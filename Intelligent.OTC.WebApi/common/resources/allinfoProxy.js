angular.module('resources.allinfoProxy', []);
angular.module('resources.allinfoProxy').factory('allinfoProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('allinfo');

    //string invoiceState, string invoiceTrackState, string invoiceNum, string soNum, string poNum, string invoiceMemo
    factory.allinfoPaging = function (index, itemCount, filter ,successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= CustomerNum asc" + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    return factory;
} ]);
