angular.module('resources.userProxy', []);
angular.module('resources.userProxy').factory('userProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('users');

    factory.searchPaing = function (filterStr, successcb, failedcb) {
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.initUserPaging = function (index, itemCount, filter, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    };

    factory.updatePwd = function (oldPwd, newPwd, confirmPwd, successcb) {
        var password = [];
        password.push(oldPwd);
        password.push(newPwd);
        password.push(confirmPwd);
        $http({
            url: APPSETTING['serverUrl'] + '/api/user/updatePwd',
            method: 'POST',
            data: password
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };


    return factory;
} ]);