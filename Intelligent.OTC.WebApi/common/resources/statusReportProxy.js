
angular.module('resources.statusReportProxy', []);
angular.module('resources.statusReportProxy').factory('statusReportProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('statusReportProxy');

 

    factory.getStatusReport = function (valueDateF, valueDateT,successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/statusReport/getStatusReport?valueDateF=' + valueDateF + "&valueDateT=" + valueDateT,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }


    factory.export = function (valueDateF, valueDateT) {
        window.location = APPSETTING['serverUrl'] + '/api/statusReport/exporAll?valueDateF=' + valueDateF + "&valueDateT=" + valueDateT;
    }

    return factory;
} ]);
