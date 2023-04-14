angular.module('app.collectorSoa', ['ui.grid.grouping'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/collectorSoa', {
                templateUrl: 'app/soa/collectorSoa-list.tpl.html',
                controller: 'collectorSoaListCtrl',
                title:"SOA",
                resolve: {
                }
            });
    }])

    .controller('collectorSoaListCtrl',
    ['$scope', 'collectorSoaProxy', 'baseDataProxy', 'permissionProxy', 'siteProxy', 'invoiceProxy', 'modalService',
        '$interval', 'customerProxy', 'contactProxy', 'customerPaymentbankProxy', 'customerPaymentcircleProxy', 'uiGridConstants','ebProxy',
        function ($scope, collectorSoaProxy, baseDataProxy, permissionProxy, siteProxy, invoiceProxy, modalService,
            $interval, customerProxy, contactProxy, customerPaymentbankProxy, customerPaymentcircleProxy, uiGridConstants, ebProxy) {
            $scope.$parent.helloAngular = "OTC - Follow Up";
            //float menu 加载 ---alex
            $scope.floatMenuOwner = ['collectorSoaListCtrl'];

            //*************************aging list (include permission)***********************s
            //ui-grid binding
            //*****************Please Don not change the order of follow code****************
            //*****************Please Don not change the order of follow code****************
            //*****************Please Don not change the order of follow code****************
            $scope.lblbatch = false;
            $scope.lblexecute = true;
            $scope.lblfilter = true;

            $scope.divbatch = false;
            $scope.divexecute = true;
            $scope.divdone = false;
            $scope.divcomplete = false;

            $scope.searchtype = "execute";
            //var order = "&$orderby= Class asc,Risk desc,BillGroupName asc";
            //var order1 = "&$orderby= ProcessId asc,Class asc,Risk desc,BillGroupName asc";
            var order = "&$orderby= Class asc,Risk desc";
            var order1 = "&$orderby= ProcessId asc,Class asc,Risk desc";
            //var order2 = "$orderby= ExistCont asc,Class asc,Risk desc,BillGroupName asc";
            //paging init
            //execute
            $scope.maxSize = 10; //paging display max
            $scope.slexecute = 15;  //init paging size(ng-model)
            $scope.iperexecute = 15;   //init paging size(parameter)
            $scope.curpexecute = 1;     //init page
            //batch
            //$scope.slbatch = 20;  //
            //$scope.iperbatch = 20;   //
            //$scope.curpbatch = 1;     //
            //done
            $scope.sldone = 15;  //
            $scope.iperdone = 15;   //
            $scope.curpdone = 1;     //

            var filstr = "";

            $scope.totalNum = "";
            //$scope.totalARBalance = "";
            //$scope.totalPastDueAmt = "";
            //$scope.totalOver90Days = "";
            //$scope.totalCreditLimit = "";

            //paging size
            $scope.levelList = [
                { "id": 15, "levelName": '15' },
                { "id": 500, "levelName": '500' },
                { "id": 1000, "levelName": '1000' },
                { "id": 2000, "levelName": '2000' },
                { "id": 5000, "levelName": '5000' },
                { "id": 999999, "levelName": 'ALL' }
            ];

            //$scope.levelList = [
            //    { "id": 15, "levelName": '15' },
            //    { "id": 500, "levelName": '500' },
            //    { "id": 1000, "levelName": '1000' },
            //    { "id": 2000, "levelName": '2000' },
            //    { "id": 5000, "levelName": '5000' },
            //    { "id": 999999, "levelName": 'ALL' }
            //];

            $scope.menuToggle = function () {
                $("#wrapper").toggleClass("toggled");
            }
            var tempforstatus = "";       
    $scope.executeList = {
        //multiSelect: true,
        //enableFullRowSelection: true,
        //   noUnselect: true,
        columnDefs: [
            { name: 'RowNo', field: '', enableSorting: false, displayName: '', pinnedLeft: true, cellTemplate: '<span style="line-height:30px;vertical-align:middle;text-align:center;display:block;">{{grid.renderContainers.body.visibleRowCache.indexOf(row) + 1}}</span>' },

                //    {
                //        name: 'pid', displayName: 'Task NO.', width: '90'
                //, cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.GetReferenceNo(140452)">{{row.entity.id}}</a></div>'
                //    },
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '110' },
                    {
                        name: 'customerNum', displayName: 'Customer NO.', width: '120'
                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.GetReferenceNo(row.entity.processid,row.entity.customerNum,row.entity.siteUseId)">{{row.entity.customerNum}}</a></div>'
                    },
                    //{ field: 'customerNum', displayName: 'Customer NO.', width: '160' },
                    { field: 'customerName', displayName: 'Customer Name', width: '160' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '105' },
                    //{ field: 'billGroupName', displayName: 'Contact', width: '160' },
                    //{ field: 'contact', displayName: 'Contact', width: '160' },
                    {
                        name: 'collectorName', displayName: 'Contact', width: '100'
                    },
                    { field: 'class', displayName: 'Customer Class', width: '130' },
                    //{ field: 'risk', displayName: 'Ebname', width: '70', type: 'number' },
                    {
                        name: '1stPTP', displayName: 'Status', width: '250'
                        , cellTemplate: '<div class="btn-group" role="group" aria-label="First group">' +
                        '<a href="javascript:;"  ng-if="row.entity.ptP_1 > 0"   class="btn btn-success m-btn m-btn--icon" uib-tooltip="FirstConfirmPTP" tooltip-placement="right" tooltip-trigger="focus"  style="width:60px"> '+
                        '        <span>                                                                                          '+
                        '        <span class="badge badge-success">{{row.entity.ptP_1}} </span>                      '+
                        '    </span>                                                                                            '+
                        '</a>                                                                                                      '+
                        '<a href="javascript:;" ng-if="row.entity.ptP_2 > 0"  class="btn btn-info m-btn m-btn--icon" uib-tooltip="SecondConfirmPTP" tooltip-placement="right" tooltip-trigger="focus" style="width:60px">        '+
                        '    <span>                                                                                              '+
                        '        <span class="badge badge-success">{{row.entity.ptP_2}}</span>                      '+
                        '    </span>                                                                                            '+
                        '</a>                                                                                                      '+
                        '<a href="javascript:;"  ng-if="row.entity.remindering > 0" class="btn btn-warning m-btn m-btn--icon"  uib-tooltip="Reminding" tooltip-placement="right" tooltip-trigger="focus" style="width:60px">'+
                        '    <span>                                                                                              '+
                        '        <span class="badge badge-success">{{row.entity.remindering}}</span>                       '+
                        '    </span>                                                                                            '+
                        '</a>                                                                                                      '+
                        '<a href="javascript:;" ng-if="row.entity.dunning_1 > 0" class="btn btn-primary m-btn m-btn--icon" uib-tooltip="FirstOverDueFollowUp" tooltip-placement="right" tooltip-trigger="focus" style="width:60px">  '+
                        '    <span>                                                                                              '+
                        '        <span class="badge badge-success">{{row.entity.dunning_1}}</span>                      '+
                        '    </span>                                                                                            '+
                        '</a>                                                                                                      '+
                        '<a href="javascript:;" ng-if="row.entity.dunning_2 > 0" class="btn btn-danger m-btn m-btn--icon" uib-tooltip="SecondOverDueFollowUp" tooltip-placement="right" tooltip-trigger="focus" style="width:60px">  '+
                        '    <span>                                                                                              '+
                        '        <span  class="badge badge-success">{{row.entity.dunning_2}}</span>                      '+
                        '    </span>                                                                                            '+
                        '</a></div>                                                                                                      ',
                        enableColumnResizing: true

                        //,
                        //cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                        //    if (parseFloat(grid.getCellValue(row, col)) > 0) {
                        //        return 'uigridred';
                        //    }
                        //}
                    },                    
                    { field: 'arBalance', displayName: 'Total A/R Balance', width: '140', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'overDueAMT', displayName: 'Over Due Amount', width: '140', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    //{
                    //    name: 'collectorName', displayName: 'Collector', width: '100'
                    //    , cellTemplate: '<div style="height:30px;vertical-align:middle;">Tom</div>'
                    //},
                    { field: 'operator', displayName: 'Collector', width: '90'},
                    //{
                    //    name: 'Sales', displayName: 'Sales', width: '140'
                    //    , cellTemplate: '<div style="height:30px;vertical-align:middle;">Yip, Doreen May Fun</div>'
                    //},
                    { field: 'sales', displayName: 'Sales', width: '120'},
                    {
                        name: 'cs', displayName: 'CS', width: '60'
                    },
                    //{ field: 'fDueOver90Amt', displayName: 'Collector', width: '160', cellFilter: 'number:2', type: 'number' },
                    
                    //{ field: 'billGroupName', displayName: 'CS', width: '160'},
                    { field: 'creditLimit', displayName: 'Credit Limit', width: '110', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'creditTrem', displayName: 'Payment Term', width: '160'},
                    //{
                    //    name: 'PaymentTermDesc', displayName: 'Payment Term', width: '120'
                    //    , cellTemplate: '<div style="height:30px;vertical-align:middle;">15 DAYS NET</div>'
                    //},
                    { field: 'due15Amt', displayName: '001-015', width: '85', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due30Amt', displayName: '031-045', width: '85', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due45Amt', displayName: '046-060', width: '85', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due60Amt', displayName: '061-090', width: '85', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due90Amt', displayName: '091-120', width: '85', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due120Amt', displayName: '120+', width: '85', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'totalFutureDue', displayName: 'Total Future Due', width: '135', cellFilter: 'number:2', type: 'number', cellClass: 'right'  },
                    { field: 'eb', displayName: 'EB Name', width: '200' }
        ],

        onRegisterApi: function (gridApi) {
            //set gridApi on scope
            $scope.gridApi = gridApi;

        }
    };

    $scope.batchList = {
        showGridFooter: true,
        showColumnFooter: true,
        enableFiltering: true,
        //enableFooterTotalSelected: false,
        columnDefs: [
                    {
                        name: 'tmp',
                        displayName: 'Execute', width: '60',
                        enableFiltering: false,
                        cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.BatchExecute(row.entity.customerNum)">Execute</a></div>'
                    },
                    { field: 'billGroupCode', displayName: 'Bill Code Group', width: '100' },
                    { field: 'billGroupName', displayName: 'Factory Group Name', width: '160' },
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '70' },
                    { field: 'customerNum', displayName: 'Customer NO.', width: '70' },
                    { field: 'customerName', displayName: 'Customer Name', width: '160' },
                    //{ field: 'contact', displayName: 'Contact', width: '160' },
                    { field: 'class', displayName: 'Customer Class', width: '70' },
                    { field: 'risk', displayName: 'Risk Score', width: '70', type: 'number', cellClass: 'right' },
                    {
                        field: 'totalAmt', displayName: 'Total A/R Balance', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    {
                        field: 'pastDueAmount', displayName: 'Past Due Amount', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    {
                        field: 'fDueOver90Amt', displayName: 'Over 90 days', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    {
                        field: 'creditLimit', displayName: 'Credit Limit', width: '80', cellFilter: 'number:2', type: 'number', cellClass: 'right', footerCellFilter: 'currency'
                        , aggregationType: uiGridConstants.aggregationTypes.sum
                    },
                    { field: 'isHoldFlg', displayName: 'Account Status', width: '80' },
                    { field: 'operator', displayName: 'Collector', width: '90' },
                    //{
                    //    field: 'contactExist', displayName: 'Check', width: '80',
                    //    cellTemplate: '<div><label ng-show="row.entity.existCont == \'N\'">no contact</label></div>'
                    //},
                    {
                        field: 'soaStatus', displayName: 'SOA Status', width: '80'
                    },
                    { field: 'failedReason', displayName: 'Failed Reason', width: '90' }
        ]
                            ,

        onRegisterApi: function (gridApi) {
            //set gridApi on scope
            $scope.gridApiBatch = gridApi;

        }
    };

    $scope.doneList = {
        columnDefs: [
                    //{
                    //    name: 'pid', displayName: 'Task NO.', width: '90'
                    //    , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.GetReferenceNo(row.entity.processId)">{{row.entity.processId}}</a></div>'
                    //},
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '140' },
                    {
                        name: 'customerNum', displayName: 'Customer NO.', width: '140'
                        , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.GetReferenceNo(row.entity.processid,row.entity.customerNum,,row.entity.siteUseId)">{{row.entity.customerNum}}</a></div>'
                    },
                    //{ field: 'customerNum', displayName: 'Customer NO.', width: '160' },
                    { field: 'customerName', displayName: 'Customer Name', width: '160' },
                    { field: 'siteUseId', displayName: 'Site Use Id', width: '160' },
                    { field: 'collectorName', displayName: 'Contact', width: '160' },
                    //{ field: 'contact', displayName: 'Contact', width: '160' },
                    { field: 'class', displayName: 'Customer Class', width: '160' },
                    //{ field: 'risk', displayName: 'Ebname', width: '70', type: 'number' },
                    {
                        name: '1stPTP', displayName: 'Status', width: '235'
                        , cellTemplate: '<div class="btn-group" role="group" aria-label="First group">' +
                        '<a href="javascript:;" class="btn btn-success m-btn m-btn--icon" > ' +
                        '        <span>                                                                                          ' +
                        '        <span class="badge badge-success">{{row.entity.ptP_1}} </span>                      ' +
                        '    </span>                                                                                            ' +
                        '</a>                                                                                                      ' +
                        '<a href="javascript:;" class="btn btn-info m-btn m-btn--icon">        ' +
                        '    <span>                                                                                              ' +
                        '        <span class="badge badge-success">{{row.entity.ptP_2}}</span>                      ' +
                        '    </span>                                                                                            ' +
                        '</a>                                                                                                      ' +
                        '<a href="javascript:;" class="btn btn-warning m-btn m-btn--icon" >' +
                        '    <span>                                                                                              ' +
                        '        <span class="badge badge-success">{{row.entity.remindering}}</span>                       ' +
                        '    </span>                                                                                            ' +
                        '</a>                                                                                                      ' +
                        '<a href="javascript:;" class="btn btn-primary m-btn m-btn--icon">  ' +
                        '    <span>                                                                                              ' +
                        '        <span class="badge badge-success">{{row.entity.dunning_1}}</span>                      ' +
                        '    </span>                                                                                            ' +
                        '</a>                                                                                                      ' +
                        '<a href="javascript:;" class="btn btn-danger m-btn m-btn--icon">  ' +
                        '    <span>                                                                                              ' +
                        '        <span class="badge badge-success">{{row.entity.dunning_2}}</span>                      ' +
                        '    </span>                                                                                            ' +
                        '</a></div>                                                                                                      ',
                        cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                            if (parseFloat(grid.getCellValue(row, col)) > 0) {
                                return 'uigridred';
                            }
                        }
                    },
                    { field: 'arBalance', displayName: 'Total A/R Balance', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'overDueAMT', displayName: 'Over Due Amount', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    //{ field: 'fDueOver90Amt', displayName: 'Collector', width: '160', cellFilter: 'number:2', type: 'number' },
                    //{ field: 'customerName', displayName: 'Sales', width: '160'},
                    //{ field: 'billGroupName', displayName: 'CS', width: '160'},
                    {
                        name: 'operator', displayName: 'Collector', width: '120'
                    },
                    //{ field: 'fDueOver90Amt', displayName: 'Collector', width: '160', cellFilter: 'number:2', type: 'number' },
                    {
                        name: 'sales', displayName: 'Sales', width: '120'
                    },
                    {
                        name: 'cs', displayName: 'CS', width: '120'
                    },
                    { field: 'creditLimit', displayName: 'Credit Limit', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    //{ field: 'fDueOver90Amt', displayName: 'Payment Term Desc', width: '160', cellFilter: 'number:2', type: 'number' },
                    {
                        name: 'creditTrem', displayName: 'Payment Term', width: '160'
                    },
                    { field: 'due15Amt', displayName: '001-015Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due30Amt', displayName: '031-045Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due45Amt', displayName: '046-060Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right'},
                    { field: 'due60Amt', displayName: '061-090Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due90Amt', displayName: '091-120Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due120Amt', displayName: '120+Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'totalFutureDue', displayName: 'Total Future Due', width: '160' }
        ]

    };
    $scope.completeList = {
        showGridFooter: true,
        showColumnFooter: true,
        enableFiltering: true,
        //enableFooterTotalSelected: false,
        columnDefs: [
            //{
            //    field: 'processId', displayName: 'Task NO.', width: '90'
            //    , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.GetReferenceNoComp(row.entity)">{{grid.appScope.CheckTask(row.entity)}}</a></div>'
            //},
            //{
            //    field: 'batchType', displayName: 'Type', width: '80'
            //    , cellTemplate: '<div style="height:30px;vertical-align:middle">{{grid.appScope.CheckType(row.entity)}}</div>'
            //    , filter: {
            //        term: '',
            //        type: uiGridConstants.filter.SELECT,
            //        selectOptions: [{ value: 1, label: 'Batch' }, { value: 2, label: 'Single' }]
            //    }, cellFilter: 'mapType'
            //},
            { field: 'legalEntity', displayName: 'Legal Entity', width: '140' },
            {
                name: 'customerNum', displayName: 'Customer NO.', width: '140'
                , cellTemplate: '<div style="height:30px;vertical-align:middle"><a style="line-height:28px" ng-click="grid.appScope.GetReferenceNoComp(row.entity.processid,row.entity.customerNum,row.entity.siteUseId)">{{row.entity.customerNum}}</a></div>'
            },
            //{ field: 'customerNum', displayName: 'Customer NO.', width: '160' },
            { field: 'customerName', displayName: 'Customer Name', width: '160' },
            { field: 'siteUseId', displayName: 'Site Use Id', width: '160' },
            { field: 'collectorName', displayName: 'Contact', width: '160' },
            //{ field: 'contact', displayName: 'Contact', width: '160' },
            { field: 'class', displayName: 'Customer Class', width: '160' },
            //{ field: 'risk', displayName: 'Ebname', width: '70', type: 'number' },
            {
                name: '1stPTP', displayName: 'Status', width: '300'
                , cellTemplate: '<div class="btn-group" role="group" aria-label="First group">' +
                '<a href="javascript:;" class="btn btn-success m-btn m-btn--icon" style="width:60px"> ' +
                '        <span>                                                                                          ' +
                '        <span class="badge badge-success">{{row.entity.ptP_1}} </span>                      ' +
                '    </span>                                                                                            ' +
                '</a>                                                                                                      ' +
                '<a href="javascript:;" class="btn btn-info m-btn m-btn--icon" style="width:60px">        ' +
                '    <span>                                                                                              ' +
                '        <span class="badge badge-success">{{row.entity.ptP_2}}</span>                      ' +
                '    </span>                                                                                            ' +
                '</a>                                                                                                      ' +
                '<a href="javascript:;" class="btn btn-warning m-btn m-btn--icon" style="width:60px">' +
                '    <span>                                                                                              ' +
                '        <span class="badge badge-success">{{row.entity.remindering}}</span>                       ' +
                '    </span>                                                                                            ' +
                '</a>                                                                                                      ' +
                '<a href="javascript:;" class="btn btn-primary m-btn m-btn--icon" style="width:60px">  ' +
                '    <span>                                                                                              ' +
                '        <span class="badge badge-success">{{row.entity.dunning_1}}</span>                      ' +
                '    </span>                                                                                            ' +
                '</a>                                                                                                      ' +
                '<a href="javascript:;" class="btn btn-danger m-btn m-btn--icon" style="width:60px">  ' +
                '    <span>                                                                                              ' +
                '        <span class="badge badge-success">{{row.entity.dunning_2}}</span>                      ' +
                '    </span>                                                                                            ' +
                '</a></div>                                                                                                      ',
                enableColumnResizing: true
                //cellClass: function (grid, row, col, rowRenderIndex, colRenderIndex) {
                //    if (parseFloat(grid.getCellValue(row, col)) > 0) {
                //        return 'uigridred';
                //    }
                //}
            },
            //{
            //    name: '1stPTP', displayName: 'Status', width: '235',
            //    cellTemplate: '<div class="btn-group" role="group" aria-label="First group">{{grid.appScope.getTemplateForStatus(row.entity.ptP_1, row.entity.ptP_2, row.entity.remindering, row.entity.dunning_1, row.entity.dunning_2)}}</div>'

            //    //cellTemplate: tempforstatus={{grid.appScope.getTemplateForStatus(row.entity.ptP_1, row.entity.ptP_2, row.entity.remindering, row.entity.dunning_1, row.entity.dunning_2) }}

            //},
            { field: 'arBalance', displayName: 'Total A/R Balance', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
            { field: 'overDueAMT', displayName: 'Over Due Amount', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    {
                        name: 'operator', displayName: 'Collector', width: '120'
                    },
                    //{ field: 'fDueOver90Amt', displayName: 'Collector', width: '160', cellFilter: 'number:2', type: 'number' },
                    {
                        name: 'sales', displayName: 'Sales', width: '120'
                    },
                    {
                        name: 'cs', displayName: 'CS', width: '120'
                    },
                    //{ field: 'customerName', displayName: 'Sales', width: '160'},
                    //{ field: 'billGroupName', displayName: 'CS', width: '160' },
                    { field: 'creditLimit', displayName: 'Credit Limit', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    {
                        name: 'creditTrem', displayName: 'Payment Term', width: '160'
                    },
                    //{ field: 'fDueOver90Amt', displayName: 'Payment Term Desc', width: '160', cellFilter: 'number:2', type: 'number' },
                    { field: 'due15Amt', displayName: '001-015Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due30Amt', displayName: '031-045Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due45Amt', displayName: '046-060Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due60Amt', displayName: '061-090Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due90Amt', displayName: '091-120Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'due120Amt', displayName: '120+Remaining', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'totalFutureDue', displayName: 'Total Future Due', width: '160', cellFilter: 'number:2', type: 'number', cellClass: 'right' }
        ]

    };

    $scope.getTemplateForStatus = function (ptP_1, ptP_2, remindering, dunning_1, dunning_2) {
        var htmlForStatus = '<div class="btn-group" role="group" aria-label="First group">';
        if (ptP_1 > 0) {
            htmlForStatus += '<a href="javascript:;" class="btn btn-success m-btn m-btn--icon" ><span><span class="badge badge-success">{{row.entity.ptP_1}} </span> ' +
                '</span></a>';
        }
        else{
            htmlForStatus += '<span><span>{{row.entity.ptP_1}}</span></span>';
        }
        if (ptP_2 > 0) {
            htmlForStatus += '<a href="javascript:;" class="btn btn-info m-btn m-btn--icon" ><span><span class="badge badge-success">{{row.entity.ptP_2}} </span> ' +
                '</span></a>';
        }
        else{
            htmlForStatus += '<span><span>{{row.entity.ptP_2}}</span></span>';
        }
        if (remindering > 0) {
            htmlForStatus += '<a href="javascript:;" class="btn btn-warning m-btn m-btn--icon" ><span><span class="badge badge-success">{{row.entity.remindering}} </span> ' +
                '</span></a>';
        }
        else {
            htmlForStatus += '<span><span>{{row.entity.remindering}}</span></span>';
        }
        if (dunning_1 > 0) {
            htmlForStatus += '<a href="javascript:;" class="btn btn-primary m-btn m-btn--icon" ><span><span class="badge badge-success">{{row.entity.dunning_1}} </span> ' +
                '</span></a>';
        }
        else{
            htmlForStatus += '<span><span>{{row.entity.dunning_1}}</span></span>';
        }
        if (dunning_2 > 0) {
            htmlForStatus += '<a href="javascript:;" class="btn btn-danger m-btn m-btn--icon" ><span><span class="badge badge-success">{{row.entity.dunning_2}} </span> ' +
                '</span></a>';
        }
        else {
            htmlForStatus += '<span><span>{{row.entity.dunning_2}}</span></span>';
        }
        htmlForStatus +='</div>';
        return htmlForStatus;
    }; 

    $scope.init = function () {

        collectorSoaProxy.soaPaging(1, 15, order + "&customerNum=&customerName=&customerClass=&siteUseId=&invoiceState=&invoiceTrackState=&legalEntity=&invoiceNum=&soNum=&poNum=&invoiceMemo=&EB=&", function (list) {
            $scope.ttexecute = list[0].count; //init count
            $scope.executeList.data = list[0].results; //init list

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            //float menu 加载 ---alex
            $scope.$broadcast("FLOAT_MENU_REFRESH", $scope.floatMenuOwner[0]);
        }, function (error) {
            alert(error);
        });
    }

    $scope.init();
    //*************************aging list (include permission)***********************e

    //*************************aging paging******************************************s
    //############################# execute ######################
    //paging size change
    $scope.pschangeexecute = function (selectedLevelId) {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpexecute;
        collectorSoaProxy.soaPaging(index, selectedLevelId, filstr, function (list) {
            $scope.iperexecute = selectedLevelId;
            $scope.executeList.data = list[0].results;
            $scope.ttexecute = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
        });
    };

    //paging change
    $scope.executepChanged = function () {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpexecute;
        collectorSoaProxy.soaPaging(index, $scope.iperexecute, filstr, function (list) {
            $scope.executeList.data = list[0].results;
            $scope.ttexecute = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
        }, function (error) {
            alert(error);
        });
    };
    //############################# batch ######################
    //paging size change
    //$scope.pschangebatch = function (selectedLevelId) {
    //    //multi-conditions
    //    $scope.curpbatch = 1;
    //    filstr = buildFilter();
    //    var index = $scope.curpbatch;
    //    collectorSoaProxy.soaPaging(index, selectedLevelId, filstr, function (list) {
    //        $scope.curpbatch = 1;
    //        $scope.iperbatch = selectedLevelId;
    //        $scope.batchList.data = list[0].results;
    //        $scope.ttbatch = list[0].count;
    //    });
    //};

    //paging change
    //$scope.batchpChanged = function (curpbatch) {
    //    //multi-conditions
    //    filstr = buildFilter();
    //    var index = $scope.curpbatch;
    //    collectorSoaProxy.soaPaging(curpbatch, $scope.iperbatch, filstr, function (list) {
    //        $scope.batchList.data = list[0].results;
    //        $scope.ttbatch = list[0].count;
    //    }, function (error) {
    //        alert(error);
    //    });
    //};

    //############################# done ######################
    //paging size change
    $scope.pschangedone = function (selectedLevelId) {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpdone;
        collectorSoaProxy.soaPaging(index, selectedLevelId, filstr, function (list) {
            $scope.iperdone = selectedLevelId;
            $scope.doneList.data = list[0].results;
            $scope.ttdone = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
        });
    };

    //paging change
    $scope.donepChanged = function (curpdone) {
        //multi-conditions
        filstr = buildFilter();
        var index = $scope.curpdone;
        collectorSoaProxy.soaPaging(curpdone, $scope.iperdone, filstr, function (list) {
            $scope.doneList.data = list[0].results;
            $scope.ttdone = list[0].count;

            $scope.totalNum = list[0].count;
            //$scope.totalARBalance = list[0].totalAmount;
            //$scope.totalPastDueAmt = list[0].totalPastDue;
            //$scope.totalOver90Days = list[0].totalOver90Days;
            //$scope.totalCreditLimit = list[0].totalCreditLimit;
            $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
        }, function (error) {
            alert(error);
        });
    };

    //*************************aging paging******************************************e



    //*************************aging search *****************************************s
    //openfilter
    var isShow = 0; //0:hide;1:show
    var baseFlg = true;
    $scope.openFilter = function () {

        //*************************aging filter base binding*****************************s
        if (baseFlg) {
            baseDataProxy.SysTypeDetails("004,005,006,029", function (res) {
                angular.forEach(res, function (r) {
                    $scope.istatuslist = r["004"];
                    $scope.cusstatus = r["005"];
                    $scope.cusclass = r["006"];
                    $scope.invoiceTrackStates = r["029"];
                });
            });

            //Legal Entity DropDownList binding
            siteProxy.Site("", function (legal) {
                $scope.legallist = legal;
            });
            //EB
            ebProxy.Eb("", function (eb) {
                $scope.ebList = eb;
            });

        }
        //*************************aging filter base binding*****************************e

        if (isShow == 0) {
            $("#divAgingSearch").show();
            isShow = 1;
            baseFlg = false;
        } else if (isShow == 1) {
            $("#divAgingSearch").hide();
            isShow = 0;
            baseFlg = false;
        }
    }

    //reset Search conditions
    $scope.resetSearch = function () {
        filstr = "";
        $scope.custCode = "";
        $scope.custName = "";
        $scope.class = "";
        //$scope.status = "";
        $scope.legal = "";

        $scope.billGroupCode = "";
        $scope.SiteUseId = "";
        $scope.states = "";
        $scope.trackStates = "";

        $scope.invoiceNum = "";
        $scope.soNum = "";
        $scope.poNum = "";
        $scope.invoiceMemo = "";
        $scope.eb = "";
    }

    buildFilter = function () {
        //multi-conditions
        var filterStr = "";
        if ($scope.searchtype == "execute") {
            filterStr = order ;
            //} else if ($scope.searchtype == "batch") {
            //    filterStr = order2 + "&$filter=(BatchType eq 1)";
        } else if ($scope.searchtype == "done") {
            filterStr = order1 + "&$filter=(TaskId ne '')";
        }
        //if ($scope.custCode) {
        //    filterStr += " and (contains(CustomerNum,'" + encodeURIComponent($scope.custCode) + "'))";
        //}
        if ($scope.custCode) {
            filterStr += "&customerNum=" + encodeURIComponent($scope.custCode);
        }
        else {
            filterStr += "&customerNum=";
        }

        if ($scope.custName) {
            //$scope.custName.replace(/\&/g, '%26');
            filterStr += "&customerName=" + encodeURIComponent($scope.custName);
        }
        else {
            filterStr += "&customerName=";
        }
        if ($scope.class) {
            //$scope.custName.replace(/\&/g, '%26');
            filterStr += "&customerClass=" + encodeURIComponent($scope.class);
        }
        else {
            filterStr += "&customerClass=";
        }
        if ($scope.SiteUseId) {
            //$scope.custName.replace(/\&/g, '%26');
            filterStr += "&siteUseId=" + encodeURIComponent($scope.SiteUseId);
        }
        else {
            filterStr += "&siteUseId=";
        }
        if ($scope.eb) {
            filterStr += "&EB=" + encodeURIComponent($scope.eb);
        }
        else {
            filterStr += "&EB=";
        }
        

        //if ($scope.class) {
        //    filterStr += " and (Class eq '" + $scope.class + "')";
        //}

        //if ($scope.status) {
        //    filterStr += " and (IsHoldFlg eq '" + $scope.status + "')";
        //}

        //if ($scope.legal) {
        //    filterStr += " and (contains(LegalEntity,'" + $scope.legal + "'))";
        //}

        ////invoice states
        //if ($scope.states) {
        //    filterStr += " and (contains(InvStates,'" + $scope.states + "'))";

        //}
        ////invoice Track States
        //if ($scope.trackStates) {
        //    filterStr += " and (contains(InvTrackStates,'" + $scope.trackStates + "'))";
        //}
        //bill Grop Code
        if ($scope.billGroupCode) {
            filterStr += " and (contains(BillGroupCode,'" + encodeURIComponent($scope.billGroupCode) + "'))";
        }
        //bill Grop Code
        //if ($scope.SiteUseId) {
        //    filterStr += " and (contains(SiteUseId,'" + encodeURIComponent($scope.SiteUseId) + "'))";
        //}

        //invoice Memo
        if ($scope.invoiceMemo) {
            filterStr = "&invoiceMemo=" + $scope.invoiceMemo + filterStr;
        } else {
            filterStr = "&invoiceMemo=" + filterStr;
        }

        //invoice PO
        if ($scope.poNum) {
            filterStr = "&poNum=" + $scope.poNum + filterStr;
        } else {
            filterStr = "&poNum=" + filterStr;
        }

        //invoice SO
        if ($scope.soNum) {
            filterStr = "&soNum=" + $scope.soNum + filterStr;
        } else {
            filterStr = "&soNum=" + filterStr;
        }

        //invoice Num
        if ($scope.invoiceNum) {
            filterStr = "&invoiceNum=" + $scope.invoiceNum + filterStr;
        } else {
            filterStr = "&invoiceNum=" + filterStr;
        }

        //legalentity
        if ($scope.legal) {
            filterStr += "&legalEntity=" + $scope.legal;
        } else {
            filterStr += "&legalEntity=" ;
        }

        //invoice Track States
        if ($scope.trackStates) {
            filterStr += "&invoiceTrackState=" + $scope.trackStates ;
        } else {
            filterStr += "&invoiceTrackState=" ;
        }

        //invoice states
        if ($scope.states) {
            filterStr += "&invoiceState=" + $scope.states ;
        } else {
            filterStr += "&invoiceState=" ;
        }

        return filterStr;
    };

    //Do search
    $scope.searchCollection = function () {

        //multi-conditions
        filstr = buildFilter();
        if ($scope.searchtype == "execute") {
            //current page
            $scope.curpexecute = 1;
            collectorSoaProxy.soaPaging($scope.curpexecute, $scope.iperexecute, filstr, function (list) {
                $scope.ttexecute = list[0].count;
                $scope.executeList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            }, function (error) {
                alert(error);
            });
            //} else if ($scope.searchtype == "batch") {
            //    //current page
            //    $scope.curpbatch = 1;
            //    collectorSoaProxy.soaBatch(filstr, function (list) {
            //        //$scope.ttbatch = list[0].count;
            //        $scope.batchList.data = list[0].results;
            //    }, function (error) {
            //        alert(error);
            //    });
        } else if ($scope.searchtype == "done") {
            //current page
            $scope.curpdone = 1;
            collectorSoaProxy.soaPaging($scope.curpdone, $scope.iperdone, filstr, function (list) {
                $scope.ttdone = list[0].count;
                $scope.doneList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        }

    };
    //*************************aging search *****************************************e

    //*************************Send Soa *****************************************s
    $scope.openSoa = function () {

        var strids = []; //= "";
        var groupcode = "";
        var intgroup = 0;
        //var ccount = 0;
        if ($scope.gridApi.selection.getSelectedRows()) {
            angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                if (groupcode == "") {
                    groupcode += rowItem.billGroupName;
                    //ccount++;
                    //strids.push(rowItem.customerNum);
                } else {
                    if (groupcode != rowItem.billGroupName) {
                        intgroup = 1;
                        //} else {
                        //ccount++;
                        //strids.push(rowItem.customerNum);
                    }
                }
                strids.push(rowItem.customerNum);
            });
        }
        if (strids == "") {
            alert("Please choose 1 customer at least .")
        }
        else if (intgroup != 0 && strids.length > 6) {
            alert("Please do not choose more than 6 account !");
        }
        else {
            collectorSoaProxy.checkPermission(strids, function (re) {
                if (re == 1) {
                    alert("Sorry, one or more of the chosen customers could not be executed by current collector .Please refresh the page .")
                } else {
                    if (intgroup != 0) {
                        //alert("Please do not choose 2 differet Group!");
                        if (confirm("Chosen different Group ,continue ?")) {
                            var type = "create";
                            window.open('#/soa/sendSoa/' + strids + '/' + type);
                        }
                    } else {
                        var type = "create";
                        window.open('#/soa/sendSoa/' + strids + '/' + type);
                    }
                }
            }, function (error) {
                alert(error);
            });

        }

    }

    $scope.GetReferenceNo = function (pid,custno,siteid) {
        //var strids = "";
        
        if (pid) {
            var type1 = "view";


            collectorSoaProxy.queryObject({ TaskNo: pid }, function (re) {
                strids = re.referenceNo.toString();
            }).then(function () {
                $scope.ViewSoa(strids, type1, siteid)
            });
           

        }
        else {
            var type = "create";
            window.open('#/soa/sendSoa/' + custno + '/' + type + '/' + siteid);
        }
    }
    $scope.ViewSoa = function (strids, type,siteuseid) {
        window.open('#/soa/sendSoa/' + strids + '/' + type + '/' + siteuseid);
    }

    $scope.GetReferenceNoComp = function (pid,custno,siteuseid) {
        if (pid) {

            var strids = "";
            var type = "complete";
            collectorSoaProxy.queryObject({
                TaskNo: pid
            }, function (re) {
                strids = re.referenceNo.toString();
            }).then(function () {
                $scope.ViewSoa(strids, type,siteuseid)
            });
            
        } else {
            
            var type = "complete";
            $scope.ViewSoa(custno, type, siteuseid);
        }
    }

    //*************************Send Soa *****************************************e

    $scope.dropdownvalue = "Not Send";

    //*************************Change Tab *****************************************s
    $scope.changetab = function (type) {
        if (type == "execute") {
            $scope.dropdownvalue = "Not Send";
            $("#liexecute").addClass("active");
            $("#libatch").removeClass("active");
            $("#lidone").removeClass("active");
            $("#licomplete").removeClass("active");
            $("#divFinishSearch").hide();
            $scope.lblexecute = true;
            $scope.lblbatch = false;
            $scope.lblfilter = true;
            $scope.divbatch = false;
            $scope.divexecute = true;
            $scope.divdone = false;
            $scope.divcomplete = false;
            $scope.searchtype = "execute";
            $scope.slexecute = 15;  //init paging size(ng-model)
            $scope.iperexecute = 15;   //init paging size(parameter)
            $scope.curpexecute = 1;
            collectorSoaProxy.soaPaging($scope.curpexecute, 15, order + "&customerNum=&customerName=&customerClass=&siteUseId=&invoiceState=&invoiceTrackState=&legalEntity=&invoiceNum=&soNum=&poNum=&invoiceMemo=&EB=&$filter=(SoaStatus eq 'Initialized')", function (list) {
                $scope.ttexecute = list[0].count;
                $scope.executeList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpexecute, $scope.iperexecute, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        } else if (type == "done") {
            $scope.dropdownvalue = "Work in Progress";
            $("#liexecute").removeClass("active");
            $("#libatch").removeClass("active");
            $("#lidone").addClass("active");
            $("#licomplete").removeClass("active");
            $("#divFinishSearch").hide();
            $scope.lblexecute = false;
            $scope.lblbatch = false;
            $scope.lblfilter = true;
            $scope.divbatch = false;
            $scope.divexecute = false;
            $scope.divdone = true;
            $scope.divcomplete = false;
            $scope.searchtype = "done";
            $scope.curpdone = 1;
            $scope.sldone = 15;  //
            $scope.iperdone = 15;   //
            collectorSoaProxy.soaPaging($scope.curpdone, 15, order1 + "&customerNum=&customerName=&customerClass=&siteUseId=&invoiceState=&invoiceTrackState=&legalEntity=&invoiceNum=&soNum=&poNum=&invoiceMemo=&EB=&$filter=(SoaStatus not eq 'Initialized')", function (list) {
                $scope.ttdone = list[0].count;
                $scope.doneList.data = list[0].results;

                $scope.totalNum = list[0].count;
                //$scope.totalARBalance = list[0].totalAmount;
                //$scope.totalPastDueAmt = list[0].totalPastDue;
                //$scope.totalOver90Days = list[0].totalOver90Days;
                //$scope.totalCreditLimit = list[0].totalCreditLimit;
                $scope.calculate($scope.curpdone, $scope.iperdone, list[0].results.length);
            }, function (error) {
                alert(error);
            });
        }
        //} else if (type == "batch") {
        //    $scope.dropdownvalue = "Batch Send SOA";
        //    $("#liexecute").removeClass("active");
        //    $("#libatch").addClass("active");
        //    $("#lidone").removeClass("active");
        //    $("#licomplete").removeClass("active");
        //    $("#divFinishSearch").hide();
        //    $scope.lblexecute = false;
        //    $scope.lblbatch = true;
        //    $scope.lblfilter = false;
        //    $scope.divbatch = true;
        //    $scope.divexecute = false;
        //    $scope.divdone = false;
        //    $scope.divcomplete = false;
        //    $("#divAgingSearch").hide();
        //    isShow = 0;
        //    $scope.searchtype = "batch";
        //    //$scope.curpbatch = 1;
        //    collectorSoaProxy.getNoPaging("batch", function (list) {
        //        //$scope.ttbatch = list[0].count;
        //        $scope.batchList.data = list;
        //        $interval(function () {
        //            $scope.gridApiBatch.grid.selection.selectedCount = 0;
        //            $scope.gridApiBatch.selection.selectAllRows();
        //        }, 0, 1);
        //    }, function (error) {
        //        alert(error);
        //    });

        //}
        else if (type == "complete") {
            $scope.dropdownvalue = "Work Completed";
            $("#liexecute").removeClass("active");
            $("#libatch").removeClass("active");
            $("#lidone").removeClass("active");
            $("#licomplete").addClass("active");
            $("#divFinishSearch").show();
            $scope.lblexecute = false;
            $scope.lblbatch = false;
            $scope.lblfilter = false;
            $scope.divbatch = false;
            $scope.divexecute = false;
            $scope.divdone = false;
            $scope.divcomplete = true;
            $("#divAgingSearch").hide();
            isShow = 0;
            $scope.searchtype = "complete";

            collectorSoaProxy.query({ Period: 'Period' }, function (list) {
                $scope.periodlist = list;
                $scope.period = $scope.periodlist[0].id;
            });

            collectorSoaProxy.getNoPaging("finish", function (list) {
                $scope.completeList.data = list;
            }, function (error) {
                alert(error);
            });

        }


    }

    //*************************Change Tab *****************************************e

    $scope.periodchange = function () {
        collectorSoaProxy.query({ PeriodId: $scope.period }, function (list) {
            $scope.completeList.data = list;
        }, function (error) {
            alert(error);
        });
    }


    $scope.batchsend = function () {
        if ($scope.gridApiBatch.selection.getSelectedRows()) {
            var strcusnums = [];
            angular.forEach($scope.gridApiBatch.selection.getSelectedRows(), function (rowItem) {
                strcusnums.push(rowItem.customerNum);
            });
            collectorSoaProxy.batch(strcusnums, function () {
                $scope.changetab("batch");
                alert("succeed!");
            }, function (error) {
                alert(error);
            });
        }
    }

    $scope.BatchExecute = function (cusnum) {
        if (confirm("Send Soa by single ,continue ?")) {
            var type = "create";
            window.open('#/soa/sendSoa/' + cusnum + '/' + type);
        }
    }

    $scope.CheckTask = function (obj) {
        if (obj.processId == null || obj.processId == "") {
            return obj.customerNum;
        } else {
            return obj.processId;
        }

    }

    $scope.CheckType = function (obj) {
        if (obj.batchType == 1) {
            return "Batch";
        } else {
            return "Single";
        }

    }

    //footer items calculate
    $scope.calculate = function (currentPage, itemsperpage, count) {
        if (count == 0) {
            $scope.fromItem = 0;
        } else {
            $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
        }
        $scope.toItem = (currentPage - 1) * itemsperpage + count;
    }

}])
.filter('mapType', function () {
    var typeHash = {
        1: 'Batch',
        2: 'Single'
    };
    return function (input) {
        if (!input) {
            return '';
        } else {
            return typeHash[input];
        }
    };
});