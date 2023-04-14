angular.module('resources.agingProxy', []);
angular.module('resources.agingProxy').factory('agingProxy', ['rresource', function (rresource) {
    var factory = rresource('aging');

    factory.agingPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= CustomerClass desc,CustomerNum asc" + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.agingPagingCount = function (filter, successcb, failedcb) {

        var filterStr = filter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };


    return factory;
} ]);
