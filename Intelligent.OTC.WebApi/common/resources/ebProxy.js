angular.module('resources.ebProxy', []);
angular.module('resources.ebProxy').factory('ebProxy', ['rresource', '$http', 'APPSETTING', function (rresource,
    $http, APPSETTING) {
    var factory = rresource('ebs');

    factory.Eb = function (code, successcb, failedcb) {
        return factory.query({}, successcb, failedcb);
    };
    return factory;
} ]);
