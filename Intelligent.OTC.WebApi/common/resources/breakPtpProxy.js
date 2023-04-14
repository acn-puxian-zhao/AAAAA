angular.module('resources.breakPtpProxy', []);
angular.module('resources.breakPtpProxy').factory('breakPtpProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('breakPtp');

    factory.sendEmailUrl = 'api/breakPtp';

    //Get the list of breakPTP
    factory.breakPTPPaging = function (index, itemCount, filter, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= Class asc,Risk desc,BillGroupName asc " + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    //Get a new invoiceLog Instance
    factory.getBreakPTP = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/breakPtp/getBreakPTP',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }
    //Confirm Break PTP
    factory.saveBreakPTP = function (invoLogInstance, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/breakPtp/saveBreakPTP',
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