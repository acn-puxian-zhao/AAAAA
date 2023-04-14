angular.module('resources.collectionProxy', []);
angular.module('resources.collectionProxy').factory('collectionProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('collection');

    factory.forPaging = function (index, itemCount, successcb, failedcb) {

        var itemspage = (index - 1) * 10;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage;

        return factory.odataQuery(filterStr, successcb, failedcb);

    };


    factory.reAnalyse = function (filter, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/collection/ProcessDealCollection',
            method: 'POST',
            data: filter
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
