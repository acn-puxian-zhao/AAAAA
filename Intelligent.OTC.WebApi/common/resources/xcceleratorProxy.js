angular.module('resources.xcceleratorProxy', []);
angular.module('resources.xcceleratorProxy').factory('xcceleratorProxy', ['rresource', function (rresource) {
    var factory = rresource('xccelerator');

    factory.forXccelerator = function (successcb, failedcb) {
        return factory.query(successcb, failedcb);
    };

    return factory;
} ]);