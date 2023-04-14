angular.module('resources.contactHistoryProxy', []);
angular.module('resources.contactHistoryProxy').factory('contactHistoryProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('contactHistory');

    factory.contactHistoryPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.contactHistoryPagingCount = function (filter, successcb, failedcb) {

        var filterStr = filter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.get = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contacthistory/find?contactId=' + id,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.create = function (model, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contacthistory/create',
            method: 'Post',
            data: model
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.update = function (model, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contacthistory/update',
            method: 'Post',
            data: model
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
