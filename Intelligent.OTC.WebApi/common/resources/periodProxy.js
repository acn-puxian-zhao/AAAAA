angular.module('resources.periodProxy', []);
angular.module('resources.periodProxy').factory('periodProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('peroid');

    

    factory.peroidPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= PeriodEnd desc" + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.hisAreaPeroidPaging = function (index, itemCount, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + "&$orderby= sortCode" + "&$count=true" + "&history=HisArea";
        return factory.odataQuery(filterStr, successcb, failedcb); 
    };

    factory.searchInfo = function (type, successcb)
    {
        $http({
            url: APPSETTING['serverUrl'] + '/api/peroid?Type=' + type,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.addNewperoidByEndTime = function (strEndaDt, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/peroid',
            method: 'POST',
            data: strEndaDt
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    factory.deletePeriod = function (id, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/peroid/delete?id='+id,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };
    factory.startSOA = function (type, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/peroid?Type=' + type,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getLegalHisByDate = function (searchDate, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/getLegalHisByDate?searchDate=' + searchDate,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getLegalByDash = function ( successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/getLegalByDash',
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getFileHisByDate = function (index, itemCount, searchDate, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/getFileHisByDate?pageindex=' + index + '&pagesize=' + itemCount +'&searchDate=' + searchDate,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getSubmitWaitInvDet = function (index, itemCount,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/getSubmitWaitInvDet?pageindex=' + index + '&pagesize=' + itemCount,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getSubmitWaitVat = function (index, itemCount, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/getSubmitWaitVat?pageindex=' + index + '&pagesize=' + itemCount,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.uploadAg = function (acc, inv, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/uploadAg?acc=' + acc + '&inv=' + inv ,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.uploadVat = function (vat, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/uploadVat?vat=' + vat,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.GetFileFromWebApi = function (path, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/GetFileFromWebApi?path=' + path,
            method: 'Get',
            responseType: 'arraybuffer'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getLegalNewFile = function (legal, type, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/getLegalNewFile?legal=' + legal + '&type=' + type,
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.batch = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/dataprepare/batch',
            method: 'Get'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
} ]);
