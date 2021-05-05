/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { FC } from 'react'
//import { useRuntime } from 'vtex.render-runtime'
import {
    Layout,
    PageHeader,
    Card,
    Button,
    //ButtonPlain,
    //Spinner,
    //Divider,
} from 'vtex.styleguide'
import { injectIntl, FormattedMessage, WrappedComponentProps } from 'react-intl'
//import { compose, graphql, useQuery, useMutation } from 'react-apollo'
import { useMutation } from 'react-apollo'

//import styles from './styles.css'
//import Q_LIST_CUSTOMERS from './queries/ListCustomers.gql'
import M_INIT_CONFIG from './mutations/InitConfiguration.gql'
//import M_CREATE_CUSTOMER from './mutations/CreateCustomer.gql'

const Admin: FC<WrappedComponentProps & any> = ({ intl }) => {
    //const { account, pages } = useRuntime()

    const [
        initConfig,
        { loading: initializingConfig },
    ] = useMutation(M_INIT_CONFIG)

    //const [
    //    createCustomer,
    //    { loading: creatingCustomer, data: createdCustomer },
    //] = useMutation(M_CREATE_CUSTOMER)

    return (
        <Layout
            pageHeader={
                <div className="flex justify-center">
                    <div className="w-100 mw-reviews-header">
                        <PageHeader
                            title={intl.formatMessage({
                                id: 'admin/taxjar.title',
                            })}
                        >
                        </PageHeader>
                    </div>
                </div>
            }
            fullWidth
        >
                <div>
                        <div>
                            <Card>
                                <h2>
                            <FormattedMessage id="admin/taxjar.init-config.title" />
                                </h2>
                                <p>
                                    <div className="mt4">
                                <Button
                                    variation="primary"
                                    size="regular"
                                    isLoading={initializingConfig}
                                    onClick={() => {
                                        initConfig()
                                    }}
                                    collapseLeft
                                >
                                    <FormattedMessage id="admin/taxjar.init-config.description" />{' '}
                                </Button>
                                    </div>
                                </p>
                            </Card>
                        </div>
                </div>
        </Layout>
    )
}

export default injectIntl(
    Admin
)
