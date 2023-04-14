angular.module('resources.holdCustomerProxy', []);
angular.module('resources.holdCustomerProxy').factory('holdCustomerProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('holdCustomer');

    factory.sendEmailUrl = 'api/holdCustomer';

    //Get the list of HoldCustomer
    factory.holdCustomerPaging = function (index, itemCount, filter, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= Class asc,Risk desc,BillGroupName asc " + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    //Get a new invoiceLog Instance
    factory.getHoldCustomer = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/holdCustomer/getHoldCustomer',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }
    //Confirm Break PTP
    factory.saveHoldCustomer = function (invoLogInstance, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/holdCustomer/saveHoldCustomer',
            method: 'POST',
            data: invoLogInstance
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    //send break letter
    factory.getMailInstance = function (customerNums, successcb, failedcb) {
        return factory.queryObject({ 'customerNums': customerNums }, successcb, failedcb);
    };

    return factory;
}]);