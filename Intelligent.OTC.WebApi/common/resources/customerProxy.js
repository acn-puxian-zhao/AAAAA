angular.module('resources.customerProxy', []);
angular.module('resources.customerProxy').factory('customerProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('customer');

    factory.searchcustomer = function (filterStr, successcb, failedcb) {
        
        return factory.odataQuery(filterStr, '', successcb, failedcb);

    };

    factory.getByNum = function (num, successcb, failedcb) {
        return factory.queryObject({ 'num': num }, successcb, failedcb);
    }

    factory.customerPaging = function (index, itemCount, filter, Contacter, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= CustomerNum asc " + filter + "&$count=true&Contacter=" + Contacter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.exportByCondition = function (num, name, status, collector, begintime, endtime,
        miscollector, misgroup, billcode, country, siteUseId, legalEntity, ebName, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer?custnum=' + num + '&custname=' + name+'&status='+status+
                                '&collector='+collector+'&begintime='+begintime+
                                '&endtime=' + endtime + '&miscollector=' + miscollector + '&misgroup=' + misgroup +
            '&billcode=' + billcode + '&country=' + country + '&siteUseId=' + siteUseId + '&legalEntity=' + legalEntity +
            '&ebName=' + ebName,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportComment = function ( successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/exportComment',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.exportCommentSales = function ( successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/exportCommentSales',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.finishSOA = function (filter, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/customer',
            method: 'POST',
            data: filter
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveCustomer = function (cust, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/saveCustomer',
            method: 'POST',
            data: cust
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.delCustomer = function (cust, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/delCustomer',
            method: 'POST',
            data: cust
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    //////////////////////////////////CustomerComments/////////////////////////////////////////////////
    factory.delCustomerComments = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/delCustomerComments?id=' + id,
            method: 'POST',
            data: id
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.saveCustomerComments = function (cust, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/saveCustomerComments',
            method: 'POST',
            data: cust
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    factory.addCustomerComments = function (cust, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/addCustomerComments',
            method: 'POST',
            data: cust
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getComments = function (cusNum, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/customer/searchCustomerComments?cusNum=' + cusNum,
            method: 'get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);

