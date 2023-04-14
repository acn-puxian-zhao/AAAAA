angular.module('services.cacheService', []);
angular.module('services.cacheService').service('cacheService', ['$cacheFactory', function ($cacheFactory) {

        var service = {};
        var CACHE_KEY = 'OTC';
        var cache = $cacheFactory(CACHE_KEY);

        service.get = function (key) {
            return cache.get(key);
        }

        service.put = function (key, item) {
            cache.put(key, item);
        }

        return service;
    }]);