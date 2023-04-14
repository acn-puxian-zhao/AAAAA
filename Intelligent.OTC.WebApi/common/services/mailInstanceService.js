angular.module('services.mailInstanceService', []);
angular.module('services.mailInstanceService').service('mailInstanceService', ['mailProxy', '$q', 'modalService', '$sce', function (mailProxy, $q, modalService, $sce) {

    this.getNewMail = function () {
        var defered = $q.defer();
        mailProxy.queryObject({ id: 0, type: "NE" }, function (mailInstance) {
            defered.resolve(mailInstance);
        });
        return defered.promise;
    }

    this.newMail = function () {
        var defered = $q.defer();
        mailProxy.queryObject({ id: 0, type: "NE" }, function (mailInstance) {
            //SOAFlg
            mailInstance.soaFlg = "0";
            //Bussiness_Reference
            //bussiness_Reference is del from db 20160226
           // mailInstance.bussiness_Reference = "";
            //title
            mailInstance["title"] = "Mail Create";
            var modalDefaults = {
                templateUrl: 'app/common/mail/mail-instance.tpl.html',
                controller: 'mailInstanceCtrl',
                size: 'customSize',
                resolve: {
                    custnum: function () { return ""; },
                    instance: function () { return mailInstance },
                    siteuseId: function () { return ""; },
                    invoicenums: function () { return ""; },
                    mType: function () { return "001";},
                    mailDefaults: function () {
                        return {
                            mailType: 'NE',
                            orginalMailId: 0
                        };
                    }
                },
                windowClass: 'modalDialog'
            };
            modalService.showModal(modalDefaults, {}).then(function (result) {
                defered.resolve(result);
            });
        });
        return defered.promise;
    }

    // Get view mail instance.
    this.getViewMail = function (mailId) {
        var defered = $q.defer();

        mailProxy.queryObject({ id: mailId, type: "VI" }, function (mailInstance) {
            mailInstance.title = "Mail View";
            mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);
            defered.resolve(mailInstance);
        });

        return defered.promise;
    }

    // Get view mail instance and show it in dialog.
    this.viewMail = function (mail, sentCallBack) {
        var defered = $q.defer();

        mailProxy.queryObject({ id: mail.id, type: "VI" }, function (mailInstance) {
            mailInstance.viewBody = $sce.trustAsHtml(mailInstance.body);

            //SOAFlg
            mailInstance.soaFlg = "0";
            //mailType
            mailInstance["title"] = "Mail View";

            var modalDefaults = {
                templateUrl: 'app/common/mail/mail-instance.tpl.html',
                controller: 'mailInstanceCtrl',
                size: 'customSize',
                resolve: {
                    custnum: function () { return mail.customerMailIds; },
                    instance: function () { return mailInstance },
                    mailDefaults: function () {
                        return {
                            mailType: 'VI'
                        };
                    }
                },
                windowClass: 'modalDialog'
            };
            modalService.showModal(modalDefaults, {}).then(function (result) {
                defered.resolve(result);
            });
        });
        return defered.promise;
    }

    this.getForwardMail = function (mailId) {
        var defered = $q.defer();

        mailProxy.queryObject({ id: mailId, type: "FW" }, function (mailInstance) {
            defered.resolve(mailInstance);
        });

        return defered.promise;
    }

    this.forwardMail = function (mail) {
        var defered = $q.defer();

        mailProxy.queryObject({ id: mail.id, type: "FW" }, function (mailInstance) {
            //SOAFlg
            mailInstance.soaFlg = "0";
            //mailType
            mailInstance["title"] = "Mail Forward NoCusNum"
            var modalDefaults = {
                templateUrl: 'app/common/mail/mail-instance.tpl.html',
                controller: 'mailInstanceCtrl',
                size: 'customSize',
                resolve: {
                    custnum: function () { return mail.customerMailIds; },
                    instance: function () { return mailInstance },
                    mType: function () { return "001"; },
                    mailDefaults: function () {
                        return {
                            mailType: 'FW',
                            orginalMailId: mail.id
                        };
                    }
                },
                windowClass: 'modalDialog'
            };
            modalService.showModal(modalDefaults, {}).then(function (result) {
                defered.resolve(result);
            });
        });
        return defered.promise;
    }

    // Get reply mail instance.
    this.getReplyMail = function (mailId) {
        var defered = $q.defer();

        mailProxy.queryObject({ id: mailId, type: "RE" }, function (mailInstance) {
            defered.resolve(mailInstance);
        });

        return defered.promise;
    }

    // Get reply mail instance and show it in dialog.
    this.replyMail = function (mail) {
        var defered = $q.defer();

        mailProxy.queryObject({ id: mail.id, type: "RE" }, function (mailInstance) {
            //mailType
            mailInstance["title"] = "Mail Reply NoCusNum";
            //SOAFlg
            mailInstance.soaFlg = "0";
            var modalDefaults = {
                templateUrl: 'app/common/mail/mail-instance.tpl.html',
                controller: 'mailInstanceCtrl',
                size: 'customSize',
                resolve: {
                    custnum: function () { return mail.customerMailIds; },
                    instance: function () { return mailInstance },
                    mType: function () { return "001"; },
                    mailDefaults: function () {
                        return {
                            mailType: 'RE',
                            orginalMailId: mail.id
                        };
                    }
                },
                windowClass: 'modalDialog'
            };
            modalService.showModal(modalDefaults, {}).then(function (result) {
                defered.resolve(result);
            });
        });

        return defered.promise;
    }

    var checkAddress = function (address) {
        // Pass the check if passing empty or null to check.
        if (address == null || address == "") {
            return true;
        }

        var Regex = /([\w-]{1,})@([\w-]{1,}\.)(\w.{1,})/;
        if (!Regex.test(address)) {
            return false;
        }

        return true;
    }

    this.checkAddresses = function(addresses){
        // Pass the check if passing empty or null to check.
        if (addresses == null || addresses == "") {
            return true;
        }

        var arrAddr = addresses.split(';');
        angular.forEach(arrAddr, function (address) {
            if (!checkAddress(address))
            {
                return false;
            }
        });

        return true;
    }
} ]);