angular.module('resources.initagingProxy', []);
angular.module('resources.initagingProxy').factory('initagingProxy', ['rresource', '$http', '$q', 'APPSETTING', function (rresource, $http, $q, APPSETTING) {
    var factory = rresource('initAging');

    factory.initagingPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= CustomerClass desc,CustomerNum asc" + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.initagingPagingCount = function (filter, successcb, failedcb) {
        var filterStr = filter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.initagingPagingOneYearList = function (index, itemCount, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= BillGroupCode" + "&$count=true" + "&type=a";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.deleteAgingByIds = function (ids, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/initAging',
            method: 'POST',
            data: ids
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);