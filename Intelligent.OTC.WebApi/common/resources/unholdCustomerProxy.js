angular.module('resources.unholdCustomerProxy', []);
angular.module('resources.unholdCustomerProxy').factory('unholdCustomerProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('unholdCustomer');

     //Get the list of unHoldCustomer
    factory.unholdCustomerPaging = function (index, itemCount, filter, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= Class asc,Risk desc,BillGroupName asc " + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    
    return factory;
}]);