angular.module('resources.CustomerGroupCfgProxy', []);
angular.module('resources.CustomerGroupCfgProxy').factory('CustomerGroupCfgProxy', ['rresource', function (rresource) {
    var factory = rresource('customerGroupCfg');

    factory.search = function (filterStr, successcb, failedcb) {

        return factory.odataQuery(filterStr, successcb, failedcb);

    };
    return factory;
} ]);