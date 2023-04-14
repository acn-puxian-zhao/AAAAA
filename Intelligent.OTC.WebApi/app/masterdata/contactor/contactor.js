angular.module('app.masterdata.contactor', [])

    .controller('contactorEditCtrl', ['$scope', 'cont', '$uibModalInstance', 'num', 'site', 'langs', 'isHoldStatus', 'contactProxy', 'baseDataProxy',
        'legal',
        function ($scope, cont, $uibModalInstance, num, site, langs, isHoldStatus, contactProxy, baseDataProxy, legal) {

            $scope.tocclist = [
                { "id": '1', "toccName": 'To' },
                { "id": '2', "toccName": 'Cc' }
            ];
            $scope.isGroupList = [
                { "id": 0, "level": 'No' },
                { "id": 1, "level": 'Yes' }
            ];


            baseDataProxy.SysTypeDetail("051", function (tasktype) {
                $scope.titleList = tasktype;
            });

            if (cont.id == null) {
                //    cont.customerNum = num;
                cont.deal = site;
            }
            var conditionList = num.split(',');
            cont.customerNum = conditionList[0];
            cont.siteUseId = conditionList[1];
            $scope.languagelist = langs;
            $scope.holdlist = isHoldStatus;
            $scope.legallist = legal;
            if (cont.communicationLanguage == null) {
                cont.communicationLanguage = langs[0].detailValue;
            } else {
                cont.communicationLanguage = cont.communicationLanguage;
            }
            if (cont.isDefaultFlg == null) {
                cont.isDefaultFlg = isHoldStatus[1].detailValue;
            } else {
                cont.isDefaultFlg = cont.isDefaultFlg;
            }

            if (cont.legalEntity == null) {
                cont.legalEntity = "All";
            } else {
                cont.legalEntity = cont.legalEntity;
            }
            if (cont.toCc == null) {
                cont.toCc = "1";
            }
            if (cont.groupCode == null || cont.groupCode == "") {
                cont.isGroupLevel = 0;
            } else {
                cont.isGroupLevel = 1;
            }

            $scope.cont = cont;


            $scope.closeContact = function () {
                $uibModalInstance.close();
            };

            //$scope.onSave = function () {
            //    $uibModalInstance.close();
            //};

            $scope.updateCommon = function () {
                if ($scope.cont.legalEntity == null) {
                    $scope.cont.legalEntity = "All";
                }
                //if ($scope.cont.isGroupLevel=0) {
                //    $scope.cont.customerNum = $scope.bkcustomerNum;
                //}
                contactProxy.updateContact($scope.cont, function () { $uibModalInstance.close(); },
                    function (res) {
                        alert(res);
                    });
                //}
            };

        }])

    .controller('contactorCopyCtrl', ['$scope', '$uibModalInstance', 'contactProxy', 'customerNum', 'siteUseId','legal',
        function ($scope, $uibModalInstance, contactProxy, customerNum, siteUseId, legal) {

            $scope.siteUseId = "";
            $scope.conlist = [];
            $scope.contactList = {
                data: 'conlist',
                multiSelect: true,
                enableFullRowSelection: true,
                noUnselect: true,
                columnDefs: [
                    { field: 'name', displayName: 'Contact Name', width: '110' },
                    { field: 'emailAddress', displayName: 'Email', width: '180' },
                    { field: 'department', displayName: 'Department', width: '100' },
                    { field: 'title', displayName: 'Title' },
                    { field: 'number', displayName: 'Contact Number', width: '120' },
                    { field: 'comment', displayName: 'Comment', width: '90' },
                    {
                        field: 'groupCode', displayName: 'To/Cc',
                        cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckType(row.entity)}}</div>'
                    },
                    {
                        field: 'isGroupLevel', displayName: 'Is Group Level', width: '135',
                        cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckGroupLevel(row.entity)}}</div>', width: '90'
                    }
                ],
                onRegisterApi: function (gridApi) {
                    $scope.gridApi = gridApi;
                }
            };

            $scope.CheckType = function (obj) {
                if (obj.toCc == "1") {
                    return "To";
                } else {
                    return "Cc";
                }
            }

            $scope.CheckGroupLevel = function (obj) {
                if (obj.isGroupLevel == 1) {
                    return "Y";
                } else {
                    return "N";
                }
            }

            $scope.search = function () {
                contactProxy.getContacts($scope.siteUseId,
                    function (result) {
                        $scope.conlist= result;
                    },
                    function (res) {
                        alert(res);
                    });
            }

            $scope.closeContact = function () {
                $uibModalInstance.close();
            }

            $scope.saveContact = function () {
              
                var selected = $scope.gridApi.selection.getSelectedRows();
                if (selected && selected.length > 0) {

                    var dto = {};
                    dto.customerNum = customerNum;
                    dto.siteUseId = siteUseId;
                    dto.contactors = selected;

                    contactProxy.copyContacts(dto,
                        function () {
                            $uibModalInstance.close();
                        },
                        function (res) {
                            alert(res);
                        });
                }
                else {
                    alert("Please select contact to copy!");
                }
            };
        }])

    .controller('contactorUpdateCtrl', ['$scope',  '$uibModalInstance', 'contactProxy', 
        function ($scope, $uibModalInstance, contactProxy) {

            $scope.reset = function () {
                $scope.oldName = "";
                $scope.oldEmail = "";
                $scope.newName = "";
                $scope.newEmail = "";
            }

            $scope.update = function () {
                var dto = {};
                dto.oldName = $scope.oldName;
                dto.oldEmail = $scope.oldEmail;
                dto.newName = $scope.newName;
                dto.newEmail = $scope.newEmail;

                if (!$scope.oldName || !$scope.oldEmail|| !$scope.newName || !$scope.newEmail) {
                    alert("please input information!");
                    return;
                }

                if (($scope.oldName == $scope.oldEmail) &&( $scope.newName == $scope.newEmail)) {
                    alert("please input a new value!");
                    return;
                }

                contactProxy.batchUpdate(dto,
                    function (res) {
                        alert("update " + res + " Successful!");
                    },
                    function (res) {
                        alert(res);
                    });
            }

            $scope.close = function () {
                $uibModalInstance.close();
            };

            $scope.reset();
        }
    ]);
    ;

