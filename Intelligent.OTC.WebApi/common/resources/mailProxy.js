angular.module('resources.mailProxy', []);
angular.module('resources.mailProxy').factory('mailProxy', ['rresource', '$http', 'APPSETTING', function (rresource, $http, APPSETTING) {
    var factory = rresource('mail');
    factory.sendMailUrl = 'api/mail';
    factory.saveMailUrl = 'api/mail/saveMail';
    factory.deleteMailUrl = 'api/mail/deletemail';

    factory.sendMail = function (code, successcb, failedcb) {
        return factory.query({ 'strTypecode': code }, successcb, failedcb);
    };

    factory.sendMail = function (url, mailInstance, successcb) {
        var apiUrl = '';
        if (typeof url == "undefined") {
            apiUrl = factory.sendMailUrl;
        } else {
            apiUrl = url;
        }
        var httpPromise = $http({
            url: apiUrl,
            method: 'POST',
            data: mailInstance
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    }

    factory.saveMail = function (mailInstance, successcb) {

        var httpPromise = $http({
            url: factory.saveMailUrl,
            method: 'POST',
            data: mailInstance
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getMailInstanceByTemplateId = function (templateId, successcb, failedcb) {
        return factory.queryObject({ templateId: templateId });
    }
    factory.getMailInstanceByTemplateId = function (language, templateId, successcb, failedcb) {
        return factory.queryObject({ templateType: templateId, language: language });
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

    factory.queryMails = function (input, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/mail/querymails',
            method: 'POST',
            data: input
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

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

    factory.queryMailCount = function (successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/mail/querycount',
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getMailCountDistinct = function (category, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Mail?category=' + category,
            method: 'GET'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    factory.getMailInvoicebyCus = function (customer, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Mail/GetInvoiceByMailId',
            method: 'POST',
            data: customer
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
    };

    //assign customer
    factory.assignCustomer = function (id, cusNum, siteUseId, successcb) {

        $http({
            url: APPSETTING['serverUrl'] + '/api/Mail?id=' + id + '&cusNum=' + cusNum + '&siteUseId=' + siteUseId,
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
    factory.getMailInstance = function (customerNums, siteUseId, successcb, failedcb) {
        return factory.queryObject({ 'customerNums': customerNums, siteUseId: siteUseId }, successcb, failedcb);
    };

    //MailDetail:remove customer from DB
    factory.removeCus = function (messageId, cusNum, siteUseId, successcb) {
        $http({
            url: APPSETTING['serverUrl'] + '/api/Mail?messageId=' + messageId + '&cusNum=' + cusNum + '&siteUseId=' + siteUseId,
            method: 'POST'
        }).then(function (result) {
            successcb(result.data);
        }).catch(function (result) {
            alert(result.data);
        });
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
