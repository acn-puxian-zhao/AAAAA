angular.module('resources.disputeTrackingProxy', []);
angular.module('resources.disputeTrackingProxy').factory('disputeTrackingProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('disputeTracking');
    factory.sendEmailUrl = 'api/disputeTracking/sendMail';

    factory.GetSOAMailInstance = function (customerNums, siteUseId, temptype, intIds, fileType, successcb ) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/disputeTracking/generate?customerNums=' + customerNums + '&siteUseId=' + siteUseId + '&temptype=' + temptype + '&fileType=' + fileType,
            method: 'POST',
            data: intIds
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    factory.disputeTrackingPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= Status, CreateDate " + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    //save Special Notes
    factory.savenotes = function (list, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/disputeTracking',
            method: 'POST',
            data: list
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
