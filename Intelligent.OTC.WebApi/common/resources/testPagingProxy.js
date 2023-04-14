angular.module('resources.testPagingProxy', []);
angular.module('resources.testPagingProxy').factory('testPagingProxy', ['rresource', function (rresource) {
    var factory = rresource('testPaging');

    factory.fortestPaging = function (index, itemCount, filter, successcb, failedcb) {
        var itemspage = (index - 1) * 10;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter;
        return factory.odataQuery(filterStr, successcb, failedcb);

    };
    factory.pagecount = function (filter, successcb, failedcb) {
        var filterStr = filter;
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.searchPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * 10;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter;
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.newSearchPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * 10;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter + "&$inlinecount=allpages";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };


    return factory;
} ]);
