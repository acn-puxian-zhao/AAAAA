angular.module('resources.collectorSignatureProxy', []);
angular.module('resources.collectorSignatureProxy').factory('collectorSignatureProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('collectorSignature');

    factory.getCollectortSign = function (signature, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/getCustomerByCustomerNum?languageId=' + signature,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.addOrUpdateCollect = function (signature, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/collectorSignature',
            method: 'POST',
            data: signature
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
