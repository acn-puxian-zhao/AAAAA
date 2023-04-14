﻿angular.module('app.contactcust', [])
.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
} ])
    .controller('contactCustCtrl',
    ['$scope', 'modalService','num',  'istatus','contactTypeInfo','$modalInstance', 
    'invoiceProxy','baseDataProxy','contactHistoryProxy','mailc','customer',
    function ($scope, modalService,num, istatus, contactTypeInfo, $modalInstance, 
    invoiceProxy,baseDataProxy,contactHistoryProxy,mailc,customer) {
    var filstr = "";
    //$scope.cont = cont;

    //contactHistory ContactType DropDownList数据邦定
     $scope.ContactTypeDPlist = contactTypeInfo;

    //Invoice Status DropDownList数据邦定
    $scope.istatus = istatus;
    //Payment Type DropDownList数据邦定
    baseDataProxy.SysTypeDetail("010", function (paymenttypelist) {
        $scope.paymenttypelist = paymenttypelist;
        //alert($scope.paymentType);
    });
    //***************get contactHistory list************************s
     
        var filterStrSearch = " &$orderby= ContactDate desc &$filter=(CustomerNum eq '" + num + "') ";
        
        //首次加载contactHistory
        contactHistoryProxy.contactHistoryPaging(1, 5, filterStrSearch, function (contactHistoryLi) {
            $scope.listHistory = contactHistoryLi[0].results; //首次当前页数据
            $scope.totalItems = contactHistoryLi[0].count; //查询结果初始化数量
        });

        $scope.itemsperpage = 5;
        $scope.currentPage = 1; //当前页
        $scope.maxSize = 5; //分页显示的最大页
        //加载nggrid数据绑定
        $scope.contactHistoryList = {
            data: 'listHistory',
            columnDefs: [
                            { field: 'contactType', displayName: 'ContactType',
                                cellTemplate: '<div hello="{valueMember: \'contactType\', basedata: \'grid.appScope.ContactTypeDPlist\'}"></div>'
                            },
                            { field: 'contacterId', displayName: 'Contact'
                            },
                            { field: 'collectorId', displayName: 'Operator' },
                            { field: 'contactDate', displayName: 'Contact Date', cellFilter: 'date:yyyy/MM/dd' },
                            { field: 'comments', displayName: 'Comments' },
                            ]
        };
    //***************get contactHistory list************************e
    
    //***************get invoice list************************s
    //首次加载invoice
    invoiceProxy.query({num:num}, function (invoicelist) {
        $scope.invlist = invoicelist; //首次当前页数据
    });
    //加载invoicenggrid数据绑定
    $scope.invoiceList = {
        data: 'invlist',

        columnDefs: [
                        { field: 'invoiceNum', displayName: 'Invoice #' ,width:'110'},
                        { field: 'customerName', displayName: 'Customer Name' ,width:'200'},
//                        { field: '', displayName: 'Collector Code' },
                        { field: 'currency', displayName: 'Invoice Currency' ,width:'100'},
                        { field: 'originalAmt', displayName: 'Orginal Invoice Amount' ,width:'100'},
                        { field: 'balanceAmt', displayName: 'Outstanding Invoice Amount' ,width:'100'},
                        { field: 'states', displayName: 'Status' ,width:'100',
                            cellTemplate: '<div hello="{valueMember: \'states\', basedata: \'grid.appScope.istatus\'}"></div>'},
                        { name: 'pt', displayName: 'Payment Type' ,width:'100'  }
                        ], 
                         onRegisterApi: function (gridApi) {
                           //set gridApi on scope
                         $scope.gridApi = gridApi;
                    }
    };
    //***************get invoice list************************e


    //***************search invoice list************************s
    $scope.searchInvoiceD = function(){
    filstr = "$filter=(CustomerNum eq '" + num + "') "; 
    if ($scope.invoiceNo) {
        filstr += " and (contains(InvoiceNum,'" + $scope.invoiceNo + "')) ";
            }
    if ($scope.invoiceStatus) {
        filstr += " and (contains(States,'" + $scope.invoiceStatus + "')) ";
            }
    if ($scope.selectedDate) {
                filstr += " and (InvoiceDate eq datetime'" + $scope.selectedDate + "')";
            }


    invoiceProxy.invoiceUnPaging(filstr, function (invoicelist) {
        $scope.invlist = invoicelist; 
    });
    
    
    }
    //***************search invoice list************************e

    //*******************************Invoice Status Update*******************************s
        $scope.InvoiceStateUpdate = function (size, status) {
            modalService.showModal({
                animation: true,
                templateUrl: 'myModalContent2.html',
                //size: size,
                controller: ['$scope', '$modalInstance', 'paymenttypelist','invselectedItems','invoiceProxy',
                                function ($scope, $modalInstance, paymenttypelist,invselectedItems,invoiceProxy) {
                                    $scope.paymenttypelist = paymenttypelist;
                                    $scope.paymentType = $scope.paymenttypelist[0].detailValue;
                                    
                                    //updateinvoice
                                    $scope.updateInvoice = function () {
                                        if(invselectedItems){
                                            var invids = "";
                                            angular.forEach(invselectedItems, function (rowItem) {
                                                if(invids==""){
                                                    invids += rowItem.id;
                                                }else{
                                                    invids += "," + rowItem.id;
                                                }
                                            });
                                            if(invids =="" ){
                                                alert("Please choose 1 invoice at least .")
                                            }else{
                                                invoiceProxy.query({ invids: invids,status:status,act:"updatestatus" }, function () {
                                                    $modalInstance.close();
                                                }), function (error) {
                                                    alert(error);
                                                }
                                            }
                                        }
                                    }

                                    //format date
                                    function changeDate()
                                    {
                                        var mydate = new Date();
                                        var str = mydate.getFullYear();
                                        if(mydate.getMonth()>9){
                                            str = str + "-" + (mydate.getMonth()+1).toString();
                                        }else{
                                        str = str + "-" + '0' + (mydate.getMonth()+1).toString();
                                        }

                                        if(mydate.getDate()>9){
                                            str = str + "-" + mydate.getDate().toString();
                                        }else{
                                        str = str + "-" + '0' + mydate.getDate().toString();
                                        }
                                        
                                        return str;
                                    }
                                    $scope.selectedDate1 = changeDate();
                                    //close 
                                    $scope.closestateupdate = function () {
                                        $modalInstance.close();
                                    }
                                } ],

                resolve: {
                    paymenttypelist: function () {
                        return $scope.paymenttypelist;
                    },
                    
                    invselectedItems : function (){
                        return $scope.gridApi.selection.getSelectedRows();
                    }
                }
            }).then(function (result){
                $scope.invselectedItems.splice(0,$scope.invselectedItems.length);
                $scope.searchInvoiceD();
            })
        };
    //*******************************Invoice Status Update*******************************e
    


    //***************window close************************s
        $scope.closeConCust = function () {
            $modalInstance.close();
        };
    //***************window close************************e

    //***************contact History*********************s
    
    //翻页
        $scope.pageChanged = function () {
            var index = $scope.currentPage;
            contactHistoryProxy.contactHistoryPaging(index, $scope.itemsperpage, filterStrSearch, function (list) {
                $scope.listHistory = list[0].results;
                $scope.totalItems = list[0].count;
            }, function (error) {
                alert(error);
            });
        };

        $scope.searchContactHis = function () {

            filterStrSearch=" &$orderby= ContactDate desc &$filter=(CustomerNum eq '" + num + "') ";
            if ($scope.contactTypeValue) {
                filterStrSearch += " and (ContactType eq '" + $scope.contactTypeValue + "')";
            }

            if ($scope.selectedDate) {
                filterStrSearch += " and (ContactDate eq datetime'" + $scope.selectedDate + "')";
            }

            contactHistoryProxy.contactHistoryPaging($scope.currentPage, $scope.itemsperpage, filterStrSearch, function (lists) {
                if (lists != null) {
                    $scope.totalItems = lists[0].count;
                    $scope.listHistory = lists[0].results;
                }
            })

        };
    //******************contact History************************e


    //*******************************showGenerateSOA*******************************s
        $scope.startsoa = function () {
            var strids = "";
            if ($scope.gridApi.selection.getSelectedRows()) {

                angular.forEach($scope.gridApi.selection.getSelectedRows(), function (rowItem) {
                    if (strids == "") {
                        strids += rowItem.id;
                    } else {
                        strids += "," + rowItem.id;
                    }
                });
            }
            if (strids == "") {
                alert("Please choose 1 invoice at least .")
            } else {
                var modalDefaults = {
                    templateUrl: 'app/aging/generatesoa/generatesoa-list.tpl.html',
                    controller: 'generatesoaCL',
                    size: 'lg',
                    resolve: {
                        custnum: function () { return num; },
                        selectedInvoiceId: function () { return strids; },
                        mailc: function() {return mailc; }
                    }

                };

                modalService.showModal(modalDefaults, {}).then(function (result) {

                });
            }
        };
        //*******************************showGenerateSOA*******************************e



    } ]);



