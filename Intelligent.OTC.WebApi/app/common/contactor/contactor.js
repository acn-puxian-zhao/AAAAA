angular.module('app.common.contactor', [])

    .controller('contactorPickupCtrl', ['$scope', 'contactProxy', 'custNum','siteUseId', 'contacterEmArr', '$uibModalInstance', '$interval',
        function ($scope, contactProxy, custNum, siteUseId, contacterEmArr, $uibModalInstance,$interval) {
        $scope.contactList = {
            data: 'conlist',
            enableFullRowSelection: true,
            columnDefs: [
                            { field: 'name', displayName: 'Contact Name' },
                            { field: 'legalEntity', displayName: 'Legal Entity' },
                            { field: 'siteUseId', displayName: 'SiteUseId' },
                            { field: 'department', displayName: 'Department' },
                            { field: 'title', displayName: 'Title' },
                            { field: 'number', displayName: 'Contact Num' },
                            { field: 'emailAddress', displayName: 'Email' },
                            {
                                field: 'toCc', displayName: 'To/Cc',
                                cellTemplate: '<div >{{grid.appScope.CheckType(row.entity)}}</div>'
                            },
//                            { name: 'op', displayName: 'Operation',
//                                cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()"><a ng-click="grid.appScope.EditContacterInfo(row.entity)"> Edit </a>&nbsp; <a ng-click="grid.appScope.Delcontacter(row.entity)"> Del </a> &nbsp; ' +
//                                '<input type="button" id="btnContact"  class="tn btn-default text-center" style="width:53px" ng-click="grid.appScope.openContactCust(row.entity)"  value="Contact" /></div>'
//                            }
            ], onRegisterApi: function (gridApi) {
                //set gridApi on scope
                $scope.gridApi = gridApi;
            }

        };
        //alert(contacterEmArr);
        //var strContacterEmArr = contacterEmArr.join(',');

//**
        contactProxy.query({ customerNums: custNum, siteUseid: siteUseId}, function (contactlist) {
            $scope.conlist = contactlist;
                $interval(function () {
                    $scope.selectItem();
                }, 0, 1);
        });

        //mapping id with name
        $scope.CheckType = function (obj) {
            if (obj.toCc == "1") {
                return "To";
            } else {
                return "Cc";
            }
        }

 //**

        //multiSelect
        //$scope.contactList.multiSelect = true;
        //$scope.contactList.isRowSelectable = function (row) {
        //    if (strContacterEmArr.indexOf(row.entity.emailAddress))
        //    { return false; }
        //    else
        //    { return true; }
        //};



        $scope.confirm = function () {
            if ($scope.gridApi.selection.getSelectedRows().length > 0) {
                //add by jiaxing distinct 
                var isEx = false;
                var arr = $scope.gridApi.selection.getSelectedRows();
                //   arr.sort();
                var res = [arr[0]];
                for (var i = 1; i < arr.length; i++) {
                    isEx = false;
                    for (var j = 0; j < res.length; j++) {
                        if (arr[i].emailAddress == res[j].emailAddress) {
                            //res.push(arr[i]);
                            isEx = true;
                            break;
                        }
                    }
                    if (!isEx) {
                        res.push(arr[i]);
                    }
                }

                $uibModalInstance.close(res);
            } else {
                $uibModalInstance.close();
            }
        };
        //$scope.confirm = function () {
        //    if ($scope.gridApi.selection.getSelectedRows().length > 0) {
        //        var arr = $scope.gridApi.selection.getSelectedRows();
        //        var cons = new Array();
        //        angular.forEach(arr, function (item) {
        //            if (cons.indexOf(item.emailAddress) < 0) {
        //                cons.push(item.emailAddress);
        //            }
        //        })
        //        $uibModalInstance.close(cons);
        //    }
        //};
        //$scope.confirm = function () {
        //    if ($scope.gridApi.selection.getSelectedRows().length > 0) {
        //        //add by jiaxing distinct 
        //        var arr = $scope.gridApi.selection.getSelectedRows();
        //        var res = [arr[0]];
        //        for (var i = 1; i < arr.length; i++) {
        //            for (var k = 0; k < res.length; k++) {
        //                if (arr[i].emailAddress !== res[res.length - 1].emailAddress) {
        //                    res.push(arr[i]);
        //                }
        //            }
        //        }
        //        $uibModalInstance.close(res);
        //    }
        //};


        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel');
        };



        

        $scope.selectItem = function () {
            for (k = 0; k < contacterEmArr.length; k++) {            
                for (i = 0; i < $scope.conlist.length; i++) {
                    var email = $scope.conlist[i].emailAddress;
                    if (contacterEmArr[k] ==email ) {
                        $scope.gridApi.selection.selectRow($scope.conlist[i]);
                    
                    }
                }
            }
        }

    } ]);

