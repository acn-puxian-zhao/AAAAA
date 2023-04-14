angular.module('app.maillist', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
        .when('/maillist', {
            templateUrl: 'app/maillist/maillist-list.tpl.html',
            controller: 'mailCtrl',
            resolve: {
            }
        });
    }])


    .controller('mailCtrl', ['$scope', 'mailInstanceService', 'cookieWatcherService', 'cookieHelper',
    function ($scope, mailInstanceService, cookieWatcherService, cookieHelper) {

        $scope.$parent.helloAngular = "OTC - Mail Box";
        //***********************************List*********************************************s
        $scope.mailInfo = [];
        $scope.mailInfo[4] = "400px"; //gridHeight
        $scope.mailInfo[6] = true; //searchSpecificUser
        $scope.custno = "";
        $scope.siteusid = "";
        //TODO: should copy instance service instead of changing the object inside the DI container.
        // override viewMail method.
        mailInstanceService.viewMail = function (mail) {
            // show special view page.
            window.open('#/maillist/maildetail/' + mail.id);
            
            var mailDetailPath = '/maillist/maildetail/' + mail.id;
            $scope.watcher = cookieWatcherService().getWatcher();
            $scope.watcher.watch(mailDetailPath);
            $scope.watcher.onDoneWatch = onDoneWatch;
            return new Object();
        }

        $scope.menuToggle = function () {
            $("#wrapper").toggleClass("toggled");
        }

        var onDoneWatch = function () {
            $scope.$broadcast("MAIL_DATAS_REFRESH");
        }

        mailInstanceService.newMail = function () {
            window.open('#/maillist/maildetail');

            var mailDetailPath = '/maillist/maildetail/new';
            $scope.watcher = cookieWatcherService().getWatcher();
            $scope.watcher.watch(mailDetailPath);
            $scope.watcher.onDoneWatch = onDoneWatch;
            var mtype = "001";
            return new Object();
        }

        $scope.MailListGrid = {
            data: 'conlist',
            //multiSelect: false,
            columnDefs: [
                //{
                //    field: 'customerName', displayName: 'Customer Name', width: '130',
                //},
                {
                    field: 'subject', displayName: 'Title', width: '500',
                    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">' +
                        '<a ng-click="grid.appScope.viewMail(row.entity)">{{row.entity.subject}}</a>' +
                        '</div>'
                },
                { field: 'from', displayName: 'From', width: '230' },
                { field: 'to', displayName: 'To', width: '230' },
                { field: 'cc', displayName: 'CC', width: '230' },
                 {
                     //field: 'mailTime', displayName: 'Date', width: '130',
                     field: 'createTime', displayName: 'Date', width: '130',
                     cellTemplate: '<div class="ngCellText">{{row.entity.createTime | date:"yyyy-MM-dd HH:mm:ss"}}</div>'
                 },
                 //{
                 //    field: 'customerNum', displayName: 'Customer NO.', width: '110',
                 //    //cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">' +
                 //    //'<a ng-click="grid.appScope.viewMail(row.entity)">{{row.entity.subject}}</a>' +
                 //    //'</div>'
                 //},
                 //{ field: 'siteUseId', displayName: 'Site Use Id', width: '120' }
                 //{
                 //    field: '""', displayName: 'Operation', width: '110',
                 //    cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()" style="height:30px;vertical-align:middle">' +
                 //                    '<a title="View" class="fa fa-list-alt" style="line-height:28px; padding-left:15px;" ng-show="grid.appScope.ViewShow" ng-click="grid.appScope.viewMail(row.entity)"></a>' +
                 //                    '<a title="Edit" class="fa fa-pencil-square-o" style="line-height:28px; padding-left:15px;" ng-show="grid.appScope.EditShow" ng-click="grid.appScope.viewMail(row.entity)"></a>' +
                 //                    '</div>'
                 //}
            ]
        };

        $scope.mailInfo[0] = $scope.MailListGrid; // mailGrid
        $scope.mailInfo[5] = mailInstanceService; //mailInstanceService

        $scope.floatMenuOwner = ['mailCtrl'];
        setTimeout(function () {
            $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
        }, 1000);
        

    }]); 