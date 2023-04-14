// Define the main module
var appRoot = angular.module('main',
    ['ngRoute', 'ngTouch', 'ui.grid', 'ui.grid.rowEdit', 'ui.grid.cellNav',  'ui.grid.resizeColumns', 'ui.grid.selection', 'ui.grid.pagination','ui.grid.edit', 'ui.grid.pinning', 'ngResource', 'ngAnimate', 'ui.uploader','ui.bootstrap', 'ui.tinymce', 'angularFileUpload', 'angularFileUploadCommon', 'ui.grid.moveColumns', 'ui.grid.infiniteScroll',
    'directives.crud.edit', 'directives.crud.buttons', 'directives.datetimepicker', 'directives.overlay', 'directives.basedataBinding', 'directives.editpartial', 'directives.mailInstance', 
    'services.breadScrumbs', 'services.crudRouteProvider', 'services.restfulResource', 'services.modalService', 'services.crud', 'services.i18nNotifications', 'services.notifications', 'services.localizedMessages', 'services.mailInstanceService', 'services.cookieWatcher', 'services.cacheService', 
        'resources.collectionProxy', 'resources.contactProxy', 'resources.contactReplaceProxy','resources.permissionProxy', 'resources.baseDataProxy', 'resources.siteProxy', 'resources.agingProxy', 'resources.initagingProxy', 'resources.invoiceProxy', 'resources.customerProxy',
    'resources.contactHistoryProxy', 'resources.userProxy', 'resources.mailTemplateProxy', 'resources.mailProxy', 'resources.generateSOAProxy', 'resources.customerPaymentbankProxy', 'resources.dunningProxy',
        'resources.CustomerGroupCfgProxy', 'resources.agingDownloadProxy', 'resources.commonProxy','resources.periodProxy', 'resources.customerPaymentcircleProxy', 'resources.collectorSoaProxy', 'resources.taskProxy' , 'resources.contactCustomerProxy', 'resources.xcceleratorProxy', 'resources.disputeTrackingProxy',
    'resources.collectorSignatureProxy', 'constants.message', 'resources.appFilesProxy', 'resources.breakPtpProxy', 'resources.holdCustomerProxy', 'resources.unholdCustomerProxy', 'resources.specialNotesProxy', 'resources.allinfoProxy', 'resources.dailyReportProxy', 'resources.myinvoicesProxy',
        'app.masterdata.contacthistory', 'app.masterdata.contactor', 'app.masterdata.ebsetting', 'app.masterdata.contactorreplace', 'app.masterdata.customer', 'app.masterdata.customerEdit', 'app.masterdata.mailtemplate', 'app.masterdata.paymentbank', 'app.masterdata.period', 'app.masterdata.reassignresponse', 'app.masterdata.user', 'app.masterdata.permissionAgent', 'resources.dashboardProxy',
        'resources.agingReportProxy', 'resources.dailyAgingProxy','resources.disputeReportProxy','resources.overdueReportProxy',
    'app.common.collector', 'app.common.contactor', 'app.common.mail', 'app.common.mailtemplate', 'app.common.floatmenu', 'app.common.paging', 
        'app.taskptpedit', 'app.taskpmtedit','app.taskdisputeedit',
        'app.dataprepare', 'app.collectorSoa', 'app.task', 'app.followup',  'app.taskedit', 'app.sendSoa', 'app.invhistory', 'app.invdetail', 'app.contactcustomer', 'app.contactMaster', 'app.common.contactdetail', 'app.common.contactdetailSecond','app.disputetracking', 'app.maillist', 'app.contactcustomeredit.contactcustomer', 'app.dunning', 'app.dispute', 'app.sendDun','app.myinvoices.vatimport',
        'app.common.import', 'app.common.importPayment', 'app.common.importContactor', 'app.common.importComment', 'app.common.exportContactor', 'app.changestatus', 'app.masterdata.dunning', 'app.allinfo', 'app.masterdata.custdomain', 'app.allaccount', 'app.maildetail', 'app.dailyReport', 'app.myinvoices', 'app.dashboard', 'app.agingreport', 'app.disputereport', 'app.overduereport', 'app.dailyAgingreport', 'app.masterdata.ar', 'app.arrowemail', 'resources.arrowemailProxy', 'directives.timeline', 'app.changeinvoicestatus',
        'app.camailalert',
        'resources.customerAssessmentLogProxy', 'resources.customerAssessmentProxy','resources.assessmentTypeProxy', 'resources.mailAccountProxy', 'resources.customerAssessmentHistoryProxy', 'resources.customerAccountPeriodProxy', 'app.masterdata.customerAccountPeriod', 'app.common.importAP', 'app.masterdata.delConfirm'
        ,'resources.ebProxy', 'app.masterdata.userUpdatePwd', 'app.ptpInfo', 'resources.PTPPaymentProxy', 'app.dso.dsoanalysis', 'resources.openARProxy', 'resources.customerContactCoverageProxy', 'app.statistics', 'resources.cashCollectedProxy', 'resources.disputReasonProxy', 'resources.statisticsCollectProxy',
        'resources.collectorStatisticsHisProxy', 'resources.reportProxy', 'app.reportOD', 'app.reportFeedback', 'app.reportFeedbackByCs', 'app.reportFeedbackBySales', 'app.reportUnApply', 'app.reportPTP', 'app.hisdata', 'resources.caHisDataProxy', 'app.uploadcafile', 'app.agentcustomer', 'app.paymentcustomer',
        'app.identifyCustomer', 'app.unknownTask', 'app.reconTask', 'app.actionTask', 'app.reconDetail', 'app.unknownDetail', 'app.cashapplication.custAndBankCust', 'app.cashapplication.customerForward', 'resources.customerForwardProxy', 'resources.custAndBankCustProxy', 'resources.caCommonProxy', 'app.cashapplication.bankstatement', 'app.cashapplication.payment', 'resources.paymentDetailProxy', 'app.cashapplication.customerAttribute', 'resources.customerAttributeProxy', 'app.cashapplication.statusReport',
        'resources.statusReportProxy', 'app.batchChangeINC', 'app.batchManualClose', 'resources.caBankFileProxy', 'app.bankfile', 'app.cashapplication.bankAttach', 'app.reuploadPost', 'app.reuploadPostClear', 'app.cashapplication.postClearResultCheck', 'app.cashapplication.bsReport', 'app.multipleResult', 'app.pmtDetail', 'resources.ebsettingProxy', 'app.error', 'app.common.importEBBranch', 'app.common.importLitigation', 'app.common.importBadDebt', 'app.customerExpirationDate', 'app.cashapplicationcountreport', 'app.cadaliyreport'
    ]);


appRoot.directive('onFinishRenderFilters', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    scope.$emit('ngRepeatFinished');
                });
            }
        }
    };
});

appRoot.config(['$routeProvider', '$locationProvider', function (routeProvider, $locationProvider) {
    $locationProvider.hashPrefix('');
    angular.lowercase = angular.$$lowercase;
    angular.uppercase = angular.$$uppercase;
    routeProvider
        .when('/', {
            templateUrl: 'app/common/report/dashboard.tpl.html',
            controller: 'dashboardCtrl',
            resolve: {
            }
        });
    routeProvider.otherwise("/error");
} ]);

// landing page
appRoot.controller('LandingPageController', ['$scope', '$location', 'breadScrumbs', 'i18nNotifications', 'localizedMessages', 'permissionProxy', 'commonProxy', 'APPSETTING', 'cacheService', function ($scope, $location, breadScrumbs, i18nNotifications, localizedMessages, permissionProxy, commonProxy, APPSETTING, cacheService) {
    $scope.helloAngular = 'Intelligent OTC platform';

    permissionProxy.all(function (allfuncs) {
        // first level: header menu
        $scope.headerMenu = allfuncs;
        $scope.currentUser = allfuncs[0].userName;


        var found = false;

        angular.forEach($scope.headerMenu, function (item) {
            var p = findRoutePage($location.path(), item);
            if (p && !found) {
                if (p.funcLevel == 1) {

                } else if (p.funcLevel == 2) {
                    $scope.pageactions = p.subFuncs;
                    found = true;
                }
            }
        });
    });

    permissionProxy.getCurrentUser('').then(function (user) {
        if (!cacheService.get('CURR_USER')) {
            cacheService.put('CURR_USER', user);
        }
    })

    var __sessionAliveTimer;

    function KeepSessionAlive() {
        // 1. Make request to server
        //new commonProxy().$save();
        //clearInterval(__sessionAliveTimer);
        //// 2. Schedule new request after 5 minutes (60000 miliseconds * 5)
        //__sessionAliveTimer = setInterval(KeepSessionAlive, 300000);
    }

    // Initial call of function
    KeepSessionAlive();

    $scope.serverUrl = APPSETTING['serverUrl'];

    /*****************header******************/
    $scope.$on('ngRepeatFinished', function (ngRepeatFinishedEvent) {
        //下面是在table render完成后执行的js
        onfinishRepeat();
    });

    function handleAuthResult(authResult) {

        if (authResult && !authResult.error) {
            // Hide auth UI, then load client library.

        } else {
            // Show auth UI, allowing the user to initiate authorization by
            // clicking authorize button.

        }
    }

    /*********************************************/

    $scope.actionClicked = function (action) {
        $scope.menuactive = false;

        // broadcast the action
        $scope.$broadcast(action.funcPage);
    };

    $scope.notifications = i18nNotifications;

    $scope.removeNotification = function (notification) {
        i18nNotifications.remove(notification);
    };

    $scope.actionClicked = function (action) {
        // broadcast the action
        $scope.$broadcast(action.funcPage);
    };

    $scope.$on('$routeChangeSuccess', function (e, current, previous) {
        $scope.activeViewPath = $location.path();
        //$scope.breadcrumbs = breadScrumbs.getAll();

        var found = false;

        angular.forEach($scope.headerMenu, function (item) {
            var p = findRoutePage($location.path(), item);
            if (p && !found) {
                if (p.funcLevel == 1) {
                    //                    $scope.leftMenu = p.subFuncs;
                    //                    if ($scope.leftMenu) {
                    //                        $scope.pageactions = $scope.leftMenu[0].subFuncs;
                    //                    }
                    //                    found = true;
                } else if (p.funcLevel == 2) {
                    $scope.pageactions = p.subFuncs;
                    found = true;
                }
            }
        });

    });

    findRoutePage = function (path, page) {
        pagepath = page.funcPage.substring(1);
        if (pagepath.length > 0 && path.match(new RegExp(pagepath))) {
            return page;
        } else {
            if (page.subFuncs) {
                for (var i = 0; i < page.subFuncs.length; i++) {
                    var p = findRoutePage(path, page.subFuncs[i]);
                    if (p) {
                        return p;
                    }
                }
            }
        }

        return null;
    };

} ]);

appRoot.config(['$httpProvider', 'APPSETTING', function ($httpProvider, APPSETTING) {
    var loginUrl = APPSETTING['loginUrl'];
    var ignoreAuth = APPSETTING['ignoreAuth'];

    $httpProvider.interceptors.push(['$rootScope', '$q', 'i18nNotifications', '$location', function ($rootScope, $q, i18nNotifications, $location) {
        return {
            responseError: function (rejection) {
                switch (rejection.status) {
                    case 401:
                        if (ignoreAuth=="false") {
                            window.location = loginUrl;
                            break;
                        }
                        return $q.reject(rejection);
                    case 403:
                        i18nNotifications.pushNow('Forbidened', 'warning');
                        return $q.reject(rejection);
                    default:
                        // otherwise, default behaviour
                        return $q.reject(rejection);
                }
            }
        };
    } ]);
} ]);