angular.module('services.restfulResource', []);
angular.module('services.restfulResource').factory('rresource', ['$http', '$q', 'APPSETTING', function ($http, $q, APPSETTING) {
    function ResoureFactory(resourceName) {

        var url = APPSETTING['serverUrl'] + '/api/' + resourceName;
        var odataUrl = APPSETTING['serverUrl'] + '/odata/' + resourceName;
        defaultParams = [];
        var thenFactoryMethod = function (httpPromise, successcb, errorcb, isArray) {
            var scb = successcb || angular.noop;
            var ecb = errorcb || angular.noop;

            return httpPromise.then(function (response) {
                var result;
                if (isArray) {
                    result = [];
                    if (response.data instanceof Array) {
                        for (var i = 0; i < response.data.length; i++) {
                            if (response.data[i].results) {
                                subResult = [];
                                for (var j = 0; j < response.data[i].results.length; j++) {
                                    subResult.push(new Resource(response.data[i].results[j]));
                                }
                                response.data[i].results = subResult;
                            }
                            result.push(new Resource(response.data[i]));
                        }
                    } else if (typeof (response.data) != 'undefined') {
                        // if actual data is not array, try to deal with it as array
                        result.push(new Resource(response.data));
                    }
                } else if (typeof (response.data) === 'string') {
                    result = response.data;
                } else {
                    result = new Resource(response.data);
                }
                scb(result, response.status, response.headers, response.config);
                return result;
            }, function (response) {
                ecb(response.data, response.status, response.headers, response.config);
                return undefined;
            });
        };

        var Resource = function (data) {
            angular.extend(this, data);
        };

        Resource.all = function (cb, errorcb) {
            return Resource.query({}, cb, errorcb);
        };

        Resource.query = function (queryJson, successcb, errorcb) {
            var params = angular.isObject(queryJson) ? JSON.stringify(queryJson) : {};
            var httpPromise = $http({
                url: url,
                method: 'GET',
                params: queryJson
            });
            return thenFactoryMethod(httpPromise, successcb, errorcb, true);
        };

        Resource.queryObject = function (queryJson, successcb, errorcb) {
            var params = angular.isObject(queryJson) ? JSON.stringify(queryJson) : {};
            var httpPromise = $http({
                url: url,
                method: 'GET',
                params: queryJson
            });
            return thenFactoryMethod(httpPromise, successcb, errorcb);
        };

        Resource.odataQuery = function (odataString, successcb, errorcb) {
            //var params = angular.isObject(queryJson) ? { q: JSON.stringify(queryJson)} : {};
            //var httpPromise = $http.post(url, { params: angular.extend({}, defaultParams, params) });
            var httpPromise = $http({
                url: odataUrl + '?' + odataString,
                method: 'GET'
            });

            return thenFactoryMethod(httpPromise, successcb, errorcb, true);
        };

        Resource.odataQueryAPI = function (odataString, successcb, errorcb) {
            //var params = angular.isObject(queryJson) ? { q: JSON.stringify(queryJson)} : {};
            //var httpPromise = $http.post(url, { params: angular.extend({}, defaultParams, params) });
            var httpPromise = $http({
                url: url + '?' + odataString,
                method: 'GET'
            });

            return thenFactoryMethod(httpPromise, successcb, errorcb, true);
        };

        Resource.getById = function (id, successcb, errorcb) {
            var httpPromise = $http.get(url + '/' + id, { params: defaultParams });
            return thenFactoryMethod(httpPromise, successcb, errorcb);
        };

        Resource.getByIds = function (ids, successcb, errorcb) {
            var qin = [];
            angular.forEach(ids, function (id) {
                qin.push({ $oid: id });
            });
            return Resource.query({ id: { $in: qin} }, successcb, errorcb);
        };

        //instance methods

        Resource.prototype.$id = function () {
            if (this.id) {//&& this._id.$oid) {
                return this.id; //.$oid;
            }
        };

        Resource.prototype.$save = function (successcb, errorcb) {
            var httpPromise = $http.post(url, this, { params: defaultParams });
            return thenFactoryMethod(httpPromise, successcb, errorcb);
        };

        Resource.prototype.$update = function (successcb, errorcb) {
            var httpPromise = $http.put(url + "/" + this.$id(), this, { params: defaultParams });
            return thenFactoryMethod(httpPromise, successcb, errorcb);
        };

        Resource.prototype.$remove = function (successcb, errorcb) {
            var httpPromise = $http['delete'](url + "/" + this.$id(), { params: defaultParams });
            return thenFactoryMethod(httpPromise, successcb, errorcb);
        };

        Resource.prototype.$saveOrUpdate = function (savecb, updatecb, errorSavecb, errorUpdatecb) {
            if (this.$id()) {
                return this.$update(updatecb, errorUpdatecb);
            } else {
                return this.$save(savecb, errorSavecb);
            }
        };

        return Resource;
    }
    return ResoureFactory;
} ]);