/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable react/jsx-no-target-blank */
/* eslint-disable react/display-name */
/* eslint-disable no-console */
import type { FC } from 'react'
import React, { useState, useEffect } from 'react'
import {
  Layout,
  PageHeader,
  PageBlock,
  Button,
  Input,
  Spinner,
  Toggle,
  Tab,
  Tabs,
  Table,
  IconPlusLines,
  Dropdown,
  Modal,
  ButtonWithIcon,
  Alert,
} from 'vtex.styleguide'
import { useIntl, FormattedMessage } from 'react-intl'
import {
  useQuery,
  useLazyQuery,
  useMutation,
  useApolloClient,
} from 'react-apollo'
import { Link } from 'vtex.render-runtime'

import M_INIT_CONFIG from './mutations/InitConfiguration.gql'
import RemoveConfiguration from './mutations/RemoveConfiguration.gql'
import ConnectionTest from './queries/connectionTest.graphql'
import AppSettings from './queries/appSettings.graphql'
import SaveAppSettings from './mutations/saveAppSettings.graphql'
import GET_CUSTOMERS from './queries/ListCustomers.gql'
import DELETE_CUSTOMER from './mutations/DeleteCustomer.gql'
import CREATE_CUSTOMER from './mutations/CreateCustomer.gql'
import VERIFY_USER from './queries/verifyEmail.graphql'
import { countries, states, options } from './common/utils'

const Admin: FC<any> = () => {
  const { formatMessage } = useIntl()
  const [settingsState, setSettingsState] = useState({
    apiToken: '',
    isLive: false,
    enableTaxCalculation: false,
    enableTransactionPosting: false,
    useTaxJarNexus: true,
    salesChannelExclude: '',
    currentTab: 1,
    updateTableKey: '',
    isModalOpen: false,
    customerName: '',
    customerEmail: '',
    customerExemptionType: '',
    customerState1: '',
    customerCountry1: '',
    customerState2: '',
    customerCountry2: '',
    customerState3: '',
    customerCountry3: '',
    customerList: [],
    customerCreationSuccess: false,
    customerCreationError: false,
    userNotFoundAlert: false,
    deleteCalled: false,
    showMoreTypes1: false,
    showMoreTypes2: false,
  })

  const { customerList } = settingsState
  const client = useApolloClient()
  const [testAllowed, setTestAllowed] = useState(false)
  const [testComplete, setTestComplete] = useState(false)
  const [settingsLoading, setSettingsLoading] = useState(false)
  const [settingsError, setSettingsError] = useState(false)
  const [settingsSaved, setSettingsSaved] = useState(false)
  const plus = <IconPlusLines />

  const { data } = useQuery(AppSettings, {
    variables: {
      version: process.env.VTEX_APP_VERSION,
    },
    ssr: false,
  })

  const [
    testConnection,
    {
      loading: connectionTestLoading,
      data: connectionTestData,
      refetch: connectionTestRefetch,
    },
  ] = useLazyQuery(ConnectionTest)

  const [
    getCustomers,
    { data: customerData, called: customerCalled },
  ] = useLazyQuery(GET_CUSTOMERS)

  const [saveSettings] = useMutation(SaveAppSettings)
  const [initConfig] = useMutation(M_INIT_CONFIG)
  const [removeConfig] = useMutation(RemoveConfiguration)
  const [createCustomer] = useMutation(CREATE_CUSTOMER)
  const [deleteCustomer] = useMutation(DELETE_CUSTOMER)

  useEffect(() => {
    if (!data?.appSettings?.message) return

    const parsedSettings = JSON.parse(data.appSettings.message)

    if (parsedSettings.apiToken) setTestAllowed(true)
    setSettingsState(parsedSettings)
  }, [data])

  const handleSaveSettings = async () => {
    setSettingsLoading(true)

    try {
      if (settingsState.enableTaxCalculation) {
        await initConfig()
      } else {
        await removeConfig()
      }

      await saveSettings({
        variables: {
          version: process.env.VTEX_APP_VERSION,
          settings: JSON.stringify(settingsState),
        },
      }).then(() => {
        setSettingsSaved(true)
        setTestAllowed(true)
      })
    } catch (error) {
      console.error(error)
      setSettingsError(true)
      setTestAllowed(false)
    } finally {
      setTestComplete(false)
      setSettingsLoading(false)
    }
  }

  const handleTestConnection = () => {
    setSettingsSaved(false)
    setSettingsError(false)
    setTestComplete(true)

    if (connectionTestData) {
      connectionTestRefetch()

      return
    }

    testConnection()
  }

  if (!customerCalled) {
    getCustomers()
  }

  if (
    customerCalled &&
    customerData &&
    customerData?.listCustomers !== customerList
  ) {
    const newList = customerData?.listCustomers

    setSettingsState({ ...settingsState, customerList: newList })
  }

  if (!settingsState.currentTab) {
    setSettingsState({
      ...settingsState,
      currentTab: 1,
    })
  }

  const handleModalToggle = () => {
    setSettingsState({
      ...settingsState,
      isModalOpen: !settingsState.isModalOpen,
    })
  }

  const handleCustomerCreate = async () => {
    const exemptRegions: any = [
      {
        state: settingsState.customerState1 || '',
        country: settingsState.customerCountry1,
      },
    ]

    if (settingsState.customerCountry2) {
      exemptRegions.push({
        state: settingsState.customerState2 || '',
        country: settingsState.customerCountry2,
      })
    }

    if (settingsState.customerCountry3) {
      exemptRegions.push({
        state: settingsState.customerState3 || '',
        country: settingsState.customerCountry3,
      })
    }

    const customer: Record<string, unknown> = {
      name: settingsState.customerName,
      customerId: settingsState.customerEmail,
      exemptionType: settingsState.customerExemptionType,
      exemptRegions,
    }

    // Check if email is registered to a VTEX account
    const query = {
      query: VERIFY_USER,
      variables: {
        email: customer.customerId,
      },
    }

    let queryRes

    try {
      queryRes = await client.query(query)
    } catch (e) {
      console.log(e)
    }

    const userData = queryRes.data.verifyEmail

    if (!userData) {
      setSettingsState({
        ...settingsState,
        userNotFoundAlert: true,
      })

      return
    }

    // Create exemption
    let res: any

    try {
      res = await createCustomer({
        variables: {
          customer,
        },
      })
    } catch (e) {
      console.log(e)
    } finally {
      if (res?.data.createCustomer) {
        const random = Math.random().toString(36).substring(7)
        const newCustomerList: any = settingsState.customerList || []

        newCustomerList.push(customer)
        setSettingsState({
          ...settingsState,
          customerList: newCustomerList,
          updateTableKey: random,
          isModalOpen: false,
          customerCreationSuccess: true,
        })
      } else {
        setSettingsState({
          ...settingsState,
          customerCreationError: true,
          isModalOpen: false,
        })
      }
    }
  }

  const handleCustomerDelete = async (rowData: any) => {
    const random = Math.random().toString(36).substring(7)
    const customers = settingsState.customerList || []
    const newCustomerList: any = customers?.filter(
      (customer: any) => customer.customerId !== rowData.customerId
    )

    setSettingsState({
      ...settingsState,
      customerList: newCustomerList,
      updateTableKey: random,
      deleteCalled: true,
    })
  }

  const lineActions = [
    {
      label: () => (
        <FormattedMessage id="admin/taxjar.settings.exemption.line-action.label" />
      ),
      isDangerous: true,
      onClick: async ({ rowData }: any) => {
        deleteCustomer({
          variables: {
            customerId: rowData.customerId,
          },
        })
          .then(() => {
            getCustomers()
          })
          .then(() => {
            handleCustomerDelete(rowData)
            alert(
              formatMessage({
                id: 'admin/taxjar.settings.exemption.line-action.alert',
              })
            )
          })
      },
    },
  ]

  const customerSchema = {
    properties: {
      name: {
        title: 'Name',
        width: 175,
      },
      id: {
        title: 'Email',
        width: 250,
        cellRenderer: (cellData: any) => {
          return <div>{cellData.rowData.customerId}</div>
        },
      },
      exemptionType: {
        title: 'Exemption Type',
        width: 150,
      },
      regions: {
        title: 'Exempt Regions',
        width: 150,
        cellRenderer: (cellData: any) => {
          const regions = cellData.rowData.exemptRegions

          return (
            <div>
              {regions.map((region: any) => {
                return (
                  <div key={region.state}>
                    {region.state}
                    {region.state ? `, ` : null}
                    {region.country}
                  </div>
                )
              })}
            </div>
          )
        },
      },
    },
  }

  if (!data) {
    return (
      <Layout
        pageHeader={
          <PageHeader title={<FormattedMessage id="admin/taxjar.title" />} />
        }
        fullWidth
      >
        <PageBlock>
          <Spinner />
        </PageBlock>
      </Layout>
    )
  }

  return (
    <Layout
      pageHeader={
        <PageHeader title={<FormattedMessage id="admin/taxjar.title" />} />
      }
      fullWidth
    >
      <PageBlock
        subtitle={
          <FormattedMessage
            id="admin/taxjar.settings.introduction"
            values={{
              tokenLink: (
                <Link
                  to="https://support.taxjar.com/article/160-how-do-i-get-a-taxjar-sales-tax-api-token"
                  target="_blank"
                >
                  <FormattedMessage id="admin/taxjar.settings.clickHere" />
                </Link>
              ),
              signupLink: (
                <Link to="https://partners.taxjar.com/English" target="_blank">
                  <FormattedMessage id="admin/taxjar.settings.clickHere" />
                </Link>
              ),
              lineBreak: <br />,
            }}
          />
        }
      >
        <Tabs>
          <Tab
            label="Settings"
            active={settingsState.currentTab === 1}
            onClick={() =>
              setSettingsState({ ...settingsState, currentTab: 1 })
            }
          >
            <section className="pb4 mt4">
              <Input
                label={formatMessage({
                  id: 'admin/taxjar.settings.apiToken.label',
                })}
                value={settingsState.apiToken}
                onChange={(e: React.FormEvent<HTMLInputElement>) =>
                  setSettingsState({
                    ...settingsState,
                    apiToken: e.currentTarget.value,
                  })
                }
                helpText={formatMessage({
                  id: 'admin/taxjar.settings.apiToken.helpText',
                })}
                token
              />
            </section>
            <section className="pv4">
              <Toggle
                semantic
                label={formatMessage({
                  id: 'admin/taxjar.settings.isLive.label',
                })}
                size="large"
                checked={settingsState.isLive}
                onChange={() => {
                  setSettingsState({
                    ...settingsState,
                    isLive: !settingsState.isLive,
                  })
                }}
                helpText={formatMessage({
                  id: 'admin/taxjar.settings.isLive.helpText',
                })}
              />
            </section>
            <section className="pv4">
              <Toggle
                semantic
                label={formatMessage({
                  id: 'admin/taxjar.settings.enableTaxCalculation.label',
                })}
                size="large"
                checked={settingsState.enableTaxCalculation}
                onChange={() => {
                  setSettingsState({
                    ...settingsState,
                    enableTaxCalculation: !settingsState.enableTaxCalculation,
                  })
                }}
                helpText={formatMessage({
                  id: 'admin/taxjar.settings.enableTaxCalculation.helpText',
                })}
              />
            </section>
            <section className="pv4">
              <Toggle
                semantic
                label={formatMessage({
                  id: 'admin/taxjar.settings.enableTransactionPosting.label',
                })}
                size="large"
                checked={settingsState.enableTransactionPosting}
                onChange={() => {
                  setSettingsState({
                    ...settingsState,
                    enableTransactionPosting: !settingsState.enableTransactionPosting,
                  })
                }}
                helpText={formatMessage({
                  id: 'admin/taxjar.settings.enableTransactionPosting.helpText',
                })}
              />
            </section>
            <section className="pv4">
              <Toggle
                semantic
                label={formatMessage({
                  id: 'admin/taxjar.settings.useTaxJarNexus.label',
                })}
                size="large"
                checked={settingsState.useTaxJarNexus}
                onChange={() => {
                  setSettingsState({
                    ...settingsState,
                    useTaxJarNexus: !settingsState.useTaxJarNexus,
                  })
                }}
                helpText={formatMessage({
                  id: 'admin/taxjar.settings.useTaxJarNexus.helpText',
                })}
              />
            </section>
            <section className="pb4">
              <Input
                label={formatMessage({
                  id: 'admin/taxjar.settings.salesChannelExclude.label',
                })}
                value={settingsState.salesChannelExclude}
                onChange={(e: React.FormEvent<HTMLInputElement>) =>
                  setSettingsState({
                    ...settingsState,
                    salesChannelExclude: e.currentTarget.value,
                  })
                }
                helpText={formatMessage({
                  id: 'admin/taxjar.settings.salesChannelExclude.helpText',
                })}
              />
            </section>
            <section className="pt4">
              <div className="mb4">
                {!settingsLoading && (settingsSaved || settingsError) ? (
                  settingsSaved ? (
                    <Alert
                      autoClose={5000}
                      type="success"
                      onClose={() => setSettingsSaved(false)}
                    >
                      <FormattedMessage id="admin/taxjar.saveSettings.success" />
                    </Alert>
                  ) : (
                    <Alert type="error" onClose={() => setSettingsError(false)}>
                      <FormattedMessage id="admin/taxjar.saveSettings.failure" />
                    </Alert>
                  )
                ) : null}
                {testComplete && !connectionTestLoading ? (
                  connectionTestData?.findProductCode?.length ? (
                    <Alert
                      autoClose={5000}
                      type="success"
                      onClose={() => setTestComplete(false)}
                    >
                      <FormattedMessage id="admin/taxjar.testConnection.success" />
                    </Alert>
                  ) : (
                    <Alert type="error" onClose={() => setTestComplete(false)}>
                      <FormattedMessage id="admin/taxjar.testConnection.failure" />
                    </Alert>
                  )
                ) : null}
              </div>
              <Button
                variation="primary"
                onClick={() => handleSaveSettings()}
                isLoading={settingsLoading}
                disabled={!settingsState.apiToken}
              >
                <FormattedMessage id="admin/taxjar.saveSettings.buttonText" />
              </Button>
            </section>
            <section className="pt4">
              <Button
                variation="secondary"
                onClick={() => handleTestConnection()}
                isLoading={connectionTestLoading}
                disabled={!testAllowed}
              >
                <FormattedMessage id="admin/taxjar.testConnection.buttonText" />
              </Button>
            </section>
          </Tab>
          <Tab
            label={formatMessage({
              id: 'admin/taxjar.settings.exemption.title',
            })}
            active={settingsState.currentTab === 2}
            onClick={() =>
              setSettingsState({ ...settingsState, currentTab: 2 })
            }
          >
            {settingsState.customerCreationSuccess && (
              <div className="mt6">
                <Alert
                  type="success"
                  onClose={() =>
                    setSettingsState({
                      ...settingsState,
                      customerCreationSuccess: false,
                    })
                  }
                >
                  <FormattedMessage id="admin/taxjar.exemption.success" />
                </Alert>
              </div>
            )}

            <div className="mt8">
              <ButtonWithIcon
                onClick={() => handleModalToggle()}
                icon={plus}
                variation="secondary"
              >
                <FormattedMessage id="admin/taxjar.settings.exemption-modal.label" />
              </ButtonWithIcon>

              {settingsState.customerCreationError && (
                <div className="mt6">
                  <Alert
                    type="error"
                    onClose={() =>
                      setSettingsState({
                        ...settingsState,
                        customerCreationError: false,
                      })
                    }
                  >
                    <FormattedMessage id="admin/taxjar.settings.exemption.customer.error" />
                  </Alert>
                </div>
              )}

              <Modal
                isOpen={settingsState.isModalOpen}
                centered
                title={formatMessage({
                  id: 'admin/taxjar.settings.exemption-modal.label',
                })}
                onClose={() => {
                  handleModalToggle()
                }}
              >
                <div className="mt4">
                  <Input
                    label={formatMessage({
                      id: 'admin/taxjar.settings.exemption-modal.name',
                    })}
                    type="string"
                    onChange={(e: any) =>
                      setSettingsState({
                        ...settingsState,
                        customerName: e.target.value,
                      })
                    }
                  />
                </div>

                <div className="mt4">
                  <Input
                    label={formatMessage({
                      id: 'admin/taxjar.settings.exemption-modal.email',
                    })}
                    type="email"
                    helpText="This email must be a VTEX account ID"
                    onChange={(e: any) =>
                      setSettingsState({
                        ...settingsState,
                        customerEmail: e.target.value,
                      })
                    }
                  />
                </div>

                {settingsState.userNotFoundAlert && (
                  <div className="mt6">
                    <Alert
                      type="error"
                      onClose={() =>
                        setSettingsState({
                          ...settingsState,
                          userNotFoundAlert: false,
                        })
                      }
                    >
                      <FormattedMessage id="admin/taxjar.settings.exemption.email.error" />
                    </Alert>
                  </div>
                )}

                <div className="mt5">
                  <Dropdown
                    label={formatMessage({
                      id: 'admin/taxjar.settings.exemption-modal.type',
                    })}
                    options={options}
                    value={settingsState.customerExemptionType}
                    size="small"
                    onChange={(_: any, v: string) =>
                      setSettingsState({
                        ...settingsState,
                        customerExemptionType: v,
                      })
                    }
                  />
                </div>

                <div className="mt6">
                  <Dropdown
                    label={formatMessage({
                      id: 'admin/taxjar.settings.exemption-modal.state',
                    })}
                    options={states}
                    size="small"
                    value={settingsState.customerState1}
                    onChange={(_: any, v: string) =>
                      setSettingsState({
                        ...settingsState,
                        customerState1: v,
                      })
                    }
                  />
                </div>

                <div className="mt4">
                  <Dropdown
                    label={formatMessage({
                      id: 'admin/taxjar.settings.exemption-modal.country',
                    })}
                    options={countries}
                    value={settingsState.customerCountry1}
                    size="small"
                    onChange={(_: any, v: string) =>
                      setSettingsState({
                        ...settingsState,
                        customerCountry1: v,
                      })
                    }
                  />
                </div>

                {!settingsState.showMoreTypes1 && (
                  <div className="mt5">
                    <ButtonWithIcon
                      onClick={() =>
                        setSettingsState({
                          ...settingsState,
                          showMoreTypes1: true,
                        })
                      }
                      icon={plus}
                      size="small"
                      variation="secondary"
                    >
                      <FormattedMessage id="admin/taxjar.settings.exemption.add-location" />
                    </ButtonWithIcon>
                  </div>
                )}

                {settingsState.showMoreTypes1 && (
                  <div>
                    <div className="mt4">
                      <Dropdown
                        label={formatMessage({
                          id: 'admin/taxjar.settings.exemption-modal.state',
                        })}
                        options={states}
                        size="small"
                        value={settingsState.customerState2}
                        onChange={(_: any, v: string) =>
                          setSettingsState({
                            ...settingsState,
                            customerState2: v,
                          })
                        }
                      />
                    </div>

                    <div className="mt4">
                      <Dropdown
                        label={formatMessage({
                          id: 'admin/taxjar.settings.exemption-modal.country',
                        })}
                        options={countries}
                        value={settingsState.customerCountry2}
                        size="small"
                        onChange={(_: any, v: string) =>
                          setSettingsState({
                            ...settingsState,
                            customerCountry2: v,
                          })
                        }
                      />
                    </div>
                  </div>
                )}

                {settingsState.showMoreTypes1 && !settingsState.showMoreTypes2 && (
                  <div className="mt5">
                    <ButtonWithIcon
                      onClick={() =>
                        setSettingsState({
                          ...settingsState,
                          showMoreTypes2: true,
                        })
                      }
                      icon={plus}
                      size="small"
                      variation="secondary"
                    >
                      <FormattedMessage id="admin/taxjar.settings.exemption.add-location" />
                    </ButtonWithIcon>
                  </div>
                )}

                {settingsState.showMoreTypes1 && settingsState.showMoreTypes2 && (
                  <div>
                    <div className="mt4">
                      <Dropdown
                        label={formatMessage({
                          id: 'admin/taxjar.settings.exemption-modal.state',
                        })}
                        options={states}
                        size="small"
                        value={settingsState.customerState3}
                        onChange={(_: any, v: string) =>
                          setSettingsState({
                            ...settingsState,
                            customerState3: v,
                          })
                        }
                      />
                    </div>

                    <div className="mt4">
                      <Dropdown
                        label={formatMessage({
                          id: 'admin/taxjar.settings.exemption-modal.country',
                        })}
                        options={countries}
                        value={settingsState.customerCountry3}
                        size="small"
                        onChange={(_: any, v: string) =>
                          setSettingsState({
                            ...settingsState,
                            customerCountry3: v,
                          })
                        }
                      />
                    </div>
                  </div>
                )}

                <div className="mt6">
                  <Button
                    onClick={() => {
                      handleCustomerCreate()
                    }}
                  >
                    <FormattedMessage id="admin/taxjar.settings.exemption-modal.submit" />
                  </Button>
                </div>
              </Modal>
            </div>
            <div className="mt5">
              <Table
                fullWidth
                updateTableKey={settingsState.updateTableKey}
                items={settingsState.customerList}
                density="low"
                schema={customerSchema}
                lineActions={lineActions}
                loading={!customerData}
              />
            </div>
          </Tab>
        </Tabs>
      </PageBlock>
    </Layout>
  )
}

export default Admin
