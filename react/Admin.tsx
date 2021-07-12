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
  ToastConsumer,
  ToastProvider,
} from 'vtex.styleguide'
import { useIntl, FormattedMessage } from 'react-intl'
import { useQuery, useLazyQuery, useMutation } from 'react-apollo'
import { Link } from 'vtex.render-runtime'

import M_INIT_CONFIG from './mutations/InitConfiguration.gql'
import RemoveConfiguration from './mutations/RemoveConfiguration.gql'
import ConnectionTest from './queries/connectionTest.graphql'
import AppSettings from './queries/appSettings.graphql'
import SaveAppSettings from './mutations/saveAppSettings.graphql'
// import Q_LIST_CUSTOMERS from './queries/ListCustomers.gql'
// import M_CREATE_CUSTOMER from './mutations/CreateCustomer.gql'

// -[Sign up for TaxJar](https://partners.taxjar.com/English)
// -[How do I get a TaxJar sales tax API token ?](https://support.taxjar.com/article/160-how-do-i-get-a-taxjar-sales-tax-api-token)

const Admin: FC = () => {
  const { formatMessage } = useIntl()
  const [settingsState, setSettingsState] = useState({
    apiToken: '',
    isLive: false,
    enableTaxCalculation: false,
    enableTransactionPosting: false,
    useTaxJarNexus: true,
    salesChannelExclude: '',
  })

  const [testAllowed, setTestAllowed] = useState(false)
  const [testComplete, setTestComplete] = useState(false)
  const [settingsLoading, setSettingsLoading] = useState(false)

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
  ] = useLazyQuery(ConnectionTest, {
    onError: () => setTestComplete(true),
    onCompleted: () => setTestComplete(true),
  })

  const [saveSettings] = useMutation(SaveAppSettings)
  const [initConfig] = useMutation(M_INIT_CONFIG)
  const [removeConfig] = useMutation(RemoveConfiguration)

  useEffect(() => {
    if (!data?.appSettings?.message) return

    const parsedSettings = JSON.parse(data.appSettings.message)

    if (parsedSettings.apiToken) setTestAllowed(true)
    setSettingsState(parsedSettings)
  }, [data])

  const handleSaveSettings = async (showToast: any) => {
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
        showToast({
          message: formatMessage({
            id: 'admin/taxjar.saveSettings.success',
          }),
          duration: 5000,
        })
        setTestAllowed(true)
        setTestComplete(false)
        setSettingsLoading(false)
      })
    } catch (error) {
      console.error(error)
      showToast({
        message: formatMessage({
          id: 'admin/taxjar.saveSettings.failure',
        }),
        duration: 5000,
      })
      setTestAllowed(false)
      setTestComplete(false)
      setSettingsLoading(false)
    }
  }

  const handleTestConnection = () => {
    if (connectionTestData) {
      connectionTestRefetch()

      return
    }

    testConnection()
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
    <ToastProvider positioning="window">
      <ToastConsumer>
        {({ showToast }: { showToast: any }) => (
          <Layout
            pageHeader={
              <PageHeader
                title={<FormattedMessage id="admin/taxjar.title" />}
              />
            }
            fullWidth
          >
            <PageBlock
              subtitle={
                <FormattedMessage
                  id="admin/taxjar.settings.introduction"
                  values={{
                    tokenLink: (
                      // eslint-disable-next-line react/jsx-no-target-blank
                      <Link
                        to="https://support.taxjar.com/article/160-how-do-i-get-a-taxjar-sales-tax-api-token"
                        target="_blank"
                      >
                        https://support.taxjar.com/ar[...]-api-token
                      </Link>
                    ),
                    signupLink: (
                      // eslint-disable-next-line react/jsx-no-target-blank
                      <Link
                        to="https://partners.taxjar.com/English"
                        target="_blank"
                      >
                        https://partners.taxjar.com/English
                      </Link>
                    ),
                    lineBreak: <br />,
                  }}
                />
              }
            >
              <section className="pb4">
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
                    id:
                      'admin/taxjar.settings.enableTransactionPosting.helpText',
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
                    id:
                      'admin/taxjar.settings.useTaxJarNexus.helpText',
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
                <Button
                  variation="primary"
                  onClick={() => handleSaveSettings(showToast)}
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
                {` `}
                {testComplete ? (
                  connectionTestData?.findProductCode?.length ? (
                    <FormattedMessage id="admin/taxjar.testConnection.success" />
                  ) : (
                    <FormattedMessage id="admin/taxjar.testConnection.failure" />
                  )
                ) : null}
              </section>
            </PageBlock>
          </Layout>
        )}
      </ToastConsumer>
    </ToastProvider>
  )
}

export default Admin
