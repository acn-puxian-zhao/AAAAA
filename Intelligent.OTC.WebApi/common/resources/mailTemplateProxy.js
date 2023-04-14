angular.module('resources.mailTemplateProxy', []);
angular.module('resources.mailTemplateProxy').factory('mailTemplateProxy', ['rresource', '$http', 'APPSETTING',
    function (rresource, $http, APPSETTING) {
        var factory = rresource('mailTemplate');

        factory.getMailTemplates = function (language, type, successcb) {
            $http({
                url: APPSETTING['serverUrl'] + '/api/MailTemplate/query?language=' + language + "&type=" + type,
                method: 'GET'
            }).then(function (result) {
                successcb(result.data);
            }).catch(function (result) {
                alert(result.data);
            });
        };

        factory.deleteTemplate = function (id, successcb) {
            $http({
                url: APPSETTING['serverUrl'] + '/api/MailTemplate/' + id,
                method: 'Delete'
            }).then(function (result) {
                successcb(result.data);
            }).catch(function (result) {
                alert(result.data);
            });
        };

        factory.saveTemplate = function (template, successcb) {
            $http({
                url: APPSETTING['serverUrl'] + '/api/MailTemplate/',
                method: 'post',
                data: template
            }).then(function (result) {
                successcb(result.data);
            }).catch(function (result) {
                alert(result.data);
            });
        };

        factory.getCusLang = function (custnum, siteUseId, successcb) {
            $http({
                url: APPSETTING['serverUrl'] + '/api/MailTemplate/getlanguage?custnum=' + custnum + '&siteUseId=' + siteUseId,
                method: 'Post'
            }).then(function (result) {
                successcb(result.data);
            }).catch(function (result) {
                alert(result.data);
            });
        };
        return factory;
    }]);
