angular.module('resources.mailAccountProxy', []);
angular.module('resources.mailAccountProxy').factory('mailAccountProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('mailAccount');
    factory.saveMailUrl = 'api/mail/saveMailAccount';

    factory.saveMailAccount = function (mailAccount, successcb) {
        var httpPromise = $http({
            url: factory.saveMailUrl,
            method: 'POST',
            data: mailAccount
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.getMailAccount = function (successcb) {
        var httpPromise = $http({
            url: 'api/mail/getMailAccount',
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getMailInstanceByTemplateId = function (templateId, successcb, failedcb) {
        return factory.queryObject({ templateId: templateId });
    }

    //add by zhangYu
    factory.initRecMailPaging = function (index, itemCount, filter, successcb, failedcb) {

        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + filter + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);

    };

    factory.searchMailPagingSp = function (index, itemCount, queryStr, orderby, successcb, failedcb) {
        var itemspage = (index - 1) * itemCount;
        var filterStr = "$top=" + itemCount + "&$skip=" + itemspage + queryStr + orderby + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    }

    factory.searchMail = function (queryStr, orderby, successcb, failedcb) {
        var filterStr = queryStr + orderby;
        return factory.odataQuery(filterStr, successcb, failedcb);
    }

    //update mail status
    factory.updateMailCategory = function (ids, category, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/Mail/updateMailCategory?category=' + category,
            method: 'POST',
            data: ids
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getMailCount = function (queryStr, successcb, failedcb) {
        var filterStr = "$top=1" + queryStr + "&$count=true";
        return factory.odataQuery(filterStr, successcb, failedcb);
    }

    //assign customer
    factory.assignCustomer = function (id, cusNum, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/Mail?id=' + id + '&cusNum=' + cusNum,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //delete selected mail
    factory.deleteSelectedMail = function (ids, successcb) {

        $http({
            url: factory.deleteMailUrl,
            method: 'POST',
            data: ids
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //send break letter
    factory.getMailInstance = function (customerNums, successcb, failedcb) {
        return factory.queryObject({ 'customerNums': customerNums }, successcb, failedcb);
    };

    //update mail customers 
    factory.updateMailCus = function (messageId, cusNums, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Mail?messageId=' + messageId + '&cusNums=' + cusNums,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    return factory;
}]);
