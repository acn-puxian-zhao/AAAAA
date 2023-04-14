angular.module('resources.contactCustomerProxy', []);
angular.module('resources.contactCustomerProxy').factory('contactCustomerProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('contactCustomer');

    factory.contactCustomerPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= Class asc,Risk desc,BillGroupName asc " + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.exportInvoiceList = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/contactCustomer?exportlist=' + "Invoice",
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //factory.mailSearch = function (index, itemCount, strCustNum, type, filter, successcb, failedcb) {
    //    var status = "contact";
    //    var filterStr = filter + "&$count=true" + "&strCustNum=" + strCustNum + "&$orderby= CreateTime desc " + "&status=" + status + "&type=" + type;
    //    return factory.odataQuery(filterStr, successcb, failedcb);

    //};

    factory.mailSearchSp = function (index, itemCount, strCustNum, type, orderby, filter, successcb, failedcb) {
        var status = "contact";
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + orderby + filter + "&$count=true" + "&strCustNum=" + strCustNum + "&status=" + status + "&type=" + type;
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    return factory;
} ]);
