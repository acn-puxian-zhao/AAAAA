angular.module('app.masterdata.customerEdit', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider
            .when('/cust/masterData/:num/', {
                templateUrl: 'app/masterdata/customer/customer-edit.tpl.html',

                controller: 'customerEditCtrl',
                resolve: {
                    languagelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("013");
                    }],
                    soatempletelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("043");
                    }],
                    regionlist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("044");
                    }],
                    isInternal: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("024");
                    }],
                    inUse: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("018");
                    }],
                    collector: ['xcceleratorProxy', function (xcceleratorProxy) {
                        return xcceleratorProxy.forXccelerator();
                    }],
                    accountStatus: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("005");
                    }],
                    overduelist: ['baseDataProxy', function (baseDataProxy) {
                        return baseDataProxy.SysTypeDetail("049");
                    }],
                    custWithGroup: ['$route', 'customerProxy', 'CustomerGroupCfgProxy', '$q', function ($route, customerProxy, CustomerGroupCfgProxy, $q) {
                        var paramsArray = $route.current.params.num.split(',');
                        var custNum = paramsArray[0];
                        var custSUI = (paramsArray[1] == undefined ? '' : paramsArray[1]);
                        var prom = $q.defer();
                        customerProxy.queryObject({ num: custNum, siteUseId: custSUI }).then(function (cust) {
                            CustomerGroupCfgProxy.search("$filter=(BillGroupCode eq '" + cust.billGroupCode + "')", function (group) {
                                var cWithG = [];
                                cWithG.push(cust);
                                if (group.length > 0) {
                                    cWithG.push(group[0]);
                                }
                                prom.resolve(cWithG);
                            });
                        });
                        return prom.promise;
                    }]
                }
            });
    }])
    .controller('commentsEditCtrl', ['$scope', 'cont', '$filter', '$uibModalInstance', 'num', 'site', 'langs', 'isHoldStatus', 'overduelist', 'customerProxy', 'baseDataProxy',
        'legal',
        function ($scope, cont, $filter, $uibModalInstance, num, site, langs, isHoldStatus, overduelist, customerProxy, baseDataProxy, legal) {
            if (cont.id == null) {
                cont.deal = site;
            }
            var conditionList = num.split(',');
            $scope.overduelist = [{ detailName: '' }];
            angular.forEach(overduelist, function (data, index, array) {
                $scope.overduelist.push(data);
            })
            $scope.agingBucketList = [{ detailName: '001-015' }, { detailName: '016-030' }, { detailName: '031-045' }, { detailName: '046-060' }, { detailName: '061-090' }, { detailName: '091-120' }, { detailName: '121-180' }, { detailName: '181-270' }, { detailName: '271-360' }, { detailName: '360+' }, { detailName: 'TotalFutureDue'}];
            $scope.commentsFromList = [{ detailName: 'Sales' }, { detailName: 'Behavior' }, { detailName: 'Both' }];
            cont.customeR_NUM = conditionList[0];
            cont.siteUseId = conditionList[1];
            console.log(cont);
            if (cont.ptpdate) {
                var valueDateStr = $filter('date')(cont.ptpdate, 'yyyy-MM-dd');
                console.log(valueDateStr);
                cont.ptpdate = valueDateStr;
                //$("#PtpDate").val(valueDateStr);
                console.log(cont);
            }

            $scope.cont = cont;

            $scope.closeContact = function () {
                $uibModalInstance.close("cancel");
            };

            $scope.updateCommon = function () {
                if (cont.id == null) {
                    customerProxy.addCustomerComments($scope.cont, function (res) {
                        if (res) {
                            alert(res);
                            return;
                        }
                        $uibModalInstance.close();
                    },
                    function (res) {
                            
                    });
                } else {
                    customerProxy.saveCustomerComments($scope.cont, function (res) {
                        if (res) {
                            alert(res);
                            return;
                        }
                        $uibModalInstance.close();
                    },
                    function (res) {
                            
                    });
                }
            };
        }])

    .controller('customerEditCtrl', ['$scope', '$routeParams', 'customerProxy', '$routeParams', 'contactProxy', 'languagelist', 'soatempletelist', 'overduelist', 'regionlist',
        'baseDataProxy', 'inUse', 'modalService', 'customerPaymentbankProxy', 'customerPaymentcircleProxy',
        'customerPaymentcircleProxy', 'accountStatus', 'FileUploader', 'APPSETTING', 'isInternal', 'CustomerGroupCfgProxy', 'collector', '$q',
        'custWithGroup', 'specialNotesProxy', 'siteProxy', 'dunningProxy', 'cacheService', '$timeout', 'customerAccountPeriodProxy', '$filter','$window',
        function ($scope, $routeParams, customerProxy, $routeParams, contactProxy, languagelist, soatempletelist, overduelist, regionlist, baseDataProxy, inUse,
            modalService, customerPaymentbankProxy, customerPaymentcircleProxy, customerPaymentcircleProxy, accountStatus,
            FileUploader, APPSETTING, isInternal, CustomerGroupCfgProxy, collector, $q, custWithGroup, specialNotesProxy,
            siteProxy, dunningProxy, cacheService, $timeout, customerAccountPeriodProxy, $filter, $window) {



            $scope.$parent.helloAngular = "OTC - Edit Customer";

            $scope.languagelist = languagelist
            $scope.soatempletelist = soatempletelist;
            $scope.regionlist = regionlist;
            $scope.overduelist = overduelist;
            $scope.inUselist = inUse;
            $scope.internallist = isInternal;
            $scope.collectorlist = collector;
            $scope.accountStatusList = accountStatus;
            $scope.AccountStatus = $scope.accountStatusList[0].id;
            $scope.cust = custWithGroup[0];
            $scope.cust.isActive = $scope.cust.status == "1";
            $scope.group = custWithGroup[1];
            $scope.cust.commentExpirationDate = $filter('date')($scope.cust.commentExpirationDate, "yyyy-MM-dd");
            $scope.cust.ptpdate = $filter('date')($scope.cust.ptpdate, "yyyy-MM-dd");
            
            //给star赋值
            $("#star").val($scope.cust.star);

            $('#star').rating({
                'showCaption': false,
                'stars': '3',
                'min': '0',
                'max': '3',
                'step': '1',
                'size': 'xs',
                'starCaptions': { 0: 'status:nix', 1: 'status:wackelt', 2: 'status:geht', 3: 'status:laeuft' }
            });


            if (!$scope.cust.contactLanguage) {
                $scope.cust.contactLanguage = $scope.languagelist[0].detailValue;
            }
            if (!$scope.cust.soaTemplete) {
                $scope.cust.soaTemplete = $scope.soatempletelist[0].detailValue;
            }

            siteProxy.GetLegalEntity('', function (list) {
                $scope.legalEntityList = list;
            }, function (res) {
                alert(res);
            });

            var legallist;

            var par = {};
            var siteUseId = '';
            var parmsArray = $routeParams.num.split(',');
            var custNum = $routeParams.num;
            par.customer_num = parmsArray[0];
            par.siteUseId = parmsArray[1];



            //获取账期年月
            customerAccountPeriodProxy.getByNumAndSiteUseId(par, function (list) {
                $scope.accountPeriodList.data = list;
            }, function (res) {
                alert(res);
            })

            $scope.accountPeriodList = {
                multiSelect: false,
                enableFullRowSelection: true,
                noUnselect: true,
                columnDefs: [
                    { field: 'customeR_NUM', displayName: 'Customer No.' },
                    { field: 'siteUseId', displayName: 'Site Use ID' },
                    { field: 'accountYear', displayName: 'Account Year' },
                    { field: 'accountMonth', displayName: 'Account Month' },
                    { field: 'reconciliationDay', displayName: 'Reconciliation Day' },
                    {
                        name: 'o', displayName: 'Operation', width: '90',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditAccountPeriodInfo(row.entity)"  class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelAccountPeriod(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                    },
                ]
            };

            $scope.EditAccountPeriodInfo = function (row) {
                var rest = $scope.getUnAddLegal;
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customerAccountPeriod/accountPeriod.tpl.html',
                    controller: 'accountPeriodCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        ap: function () {
                            var ap = custWithGroup[0];
                            return ap;
                        },
                        num: function () {
                            return $scope.cust.customerNum;
                        },
                        legal: rest,
                        typeDetailList: function () {
                            return $scope.typeDetailList;
                        },
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                });
            };

            baseDataProxy.SysTypeDetail("032", function (TypeDetails) {
                $scope.typeDetailList = TypeDetails;
            }, function () {
            });


            siteProxy.GetLegalEntity("type", function (legal) {
                $scope.legallist = legal;
                angular.forEach($scope.legallist, function (data, index, array) {
                    //var legalEntity = '';
                    //legalEntity += '<div class="row"><label class="control-label" style="font-size:inherit;width:150px;vertical-align: top;">' + data.legalEntity + ':</label>';
                    //legalEntity += '<textarea style=" width:600px;height:60px; font-size:11px;resize:none"></textarea>';
                    //legalEntity += '<input id=\'saveSpecialNotes' + index + '\' type="button" class="btn btn-primary" style="width:120px;vertical-align:top;margin-left:250px;margin-top:37px" value="Save" /></div>';
                    //legalEntity += '<div class="row" style="height:5px"></div>';
                    //$("#" + index).html(legalEntity);
                    //$("#" + index + " input").bind('click', { 'type': data.legalEntity }, saveSpecialNotes2);
                });
            }, function () {
                $("#0 textarea").html()
            });





            if ($scope.cust.excludeFlg == null) {
                $scope.cust.excludeFlg = isInternal[1].detailValue;
            } else {
                $scope.cust.excludeFlg = $scope.cust.excludeFlg;
            }
            if ($scope.cust.collector != null) {
                $scope.cust.collector = $scope.cust.collector;
            }

            $scope.formatLabel = function (model) {
                // After user selection was made, We can use the group list to find out the display group name. 
                if ($scope.groupList && $scope.groupList.$$state.value.length > 0) {
                    for (var i = 0; i < $scope.groupList.$$state.value.length; i++) {
                        if (model == $scope.groupList.$$state.value[i].billGroupCode) {
                            return $scope.groupList.$$state.value[i].billGroupName;
                        }
                    }
                } else {
                    // This method will be called during page load, We don't have group list at that time. 
                    // have to use the group we received from the page resolver.
                    if ($scope.group) {
                        return $scope.group.billGroupName;
                    }
                }
            };

            $scope.groupList;
            $scope.billGroupChange = function (code) {
                $scope.groupList = CustomerGroupCfgProxy.search("$top=10&$filter=(contains(BillGroupName,'" + code + "'))")
                return $scope.groupList;
            }

            $scope.change = function (x) {
                angular.forEach($scope.collectorlist, function (c) {
                    if (c.eid == x) {
                        $scope.cust.collectorName = c.name;
                    }
                });
            }

            //var filter = "&$filter=(contains(CustomerNum,'" + custNum + "') and (contains(SiteUseId, '" + siteUseId + "'))";
            //    customerProxy.searchcustomer(filter, function (list) {
            //        //siteNo = list[0][38];
            //    }, function (error) {
            //        alert(error);
            //    });


            contactProxy.forCustomer(custNum, function (contactlist) {
                $scope.conlist = contactlist;
                //$scope.commlist = contactlist;
            });
            customerPaymentbankProxy.forCustBank(custNum, function (custbanklist) {
                $scope.banklist = custbanklist;
            });
            customerPaymentcircleProxy.forPayDate(custNum, function (paydate) {
                $scope.paydaylist = paydate;
                //console.log(paydate[0]);
            });

            specialNotesProxy.forNotes(custNum, function (notes) {
                $scope.specialNote = notes[0].specialNotes;
                //$timeout(function () {
                //    $scope.$apply(function () {
                //        angular.forEach(notes, function (note) {
                //            angular.forEach($scope.legallist, function (legal) {
                //                if (legal.legalEntity == note.legalEntity) {
                //                    legal.specialNote = note.specialNotes;
                //                }
                //            });
                //        });
                //    })
                //}, 1000);

            });

            function safeApply(scope, fn) {
                (scope.$$phase || scope.$root.$$phase) ? fn() : scope.$apply(fn);
            }
            function setSpecialNotes(notes) {
                for (var i = 0; i < notes.length; i++) {
                    for (var j = 0; j < $("#divSpecialNotes label").length; j++) {
                        if ($("#divSpecialNotes label").eq(j).html() == notes[i].legalEntity + ":") {
                            $("#divSpecialNotes label").eq(j).siblings("textarea").html(notes[i].specialNotes);
                        }
                    }
                }
            }

            $scope.saveSpecialNotes = function (note, type) {
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    var custNo = $scope.cust.customerNum;
                    var siteUseId = $scope.cust.siteUseId;
                    var list = [];
                    list.push(custNo);
                    list.push(type);
                    list.push(note);
                    list.push(siteUseId);
                    specialNotesProxy.saveNotes(list, function (r) {
                        if (r == 1) {
                            alert("Success!");
                        }
                    }, function () {
                        alert("Failed!");
                    })
                } else {
                    alert("Please Create Customer First");
                }
            }
            //mapping id with name
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

            //                $('#popwindow').popover();
            /********************************Customer Attribute**************************************************/
            $scope.saveCustomer = function () {
                if ($("#star").val() == '' || $("#star").val() == null || $("#star").val() == undefined) {
                    $scope.cust.star = 0;
                }
                else {
                    $scope.cust.star = $("#star").val();
                }
                $scope.cust.status = $scope.cust.isActive == true ? "1" : "0";

                if (!$scope.soaTemplete) {
                    $scope.soaTemplete = "Standard";
                }

                if (!$scope.cust.siteUseId || !$scope.cust.customerNum || !$scope.cust.customerName) {
                    alert("Must input CustomerNumber & CustomerName & SiteUseId");
                    return;
                }

                customerProxy.saveCustomer($scope.cust, function (res) {
                    alert(res);
                    if (res == "Add Success!") {
                        $scope.isreadonly = true;
                        $scope.addFlg = 1;
                        //window.location = APPSETTING['serverUrl'] + "/cust/masterData/" + $scope.cust.customerNum + "," + $scope.cust.siteUseId + "/"
                    }
                });
            }

            if (custNum != 'newCust') {
                $scope.isreadonly = true;
            } else {
                $scope.isreadonly = false;
            }
            /********************************Contact List*******************************************************/

            //添加新Contacter
            $scope.AddContacterInfo = function () {
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    var num = custNum;
                    $scope.nmbNum = custNum;
                    var rest = $scope.getUnAddLegal;
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                        controller: 'contactorEditCtrl',
                        size: 'lg',
                        resolve: {
                            config: function () {
                                return new dunningProxy();
                            },
                            num: function () {
                                return $scope.cust.customerNum + ',' + $scope.cust.siteUseId;
                            },
                            legal: rest,
                            typeDetailList: function () {
                                return $scope.typeDetailList;
                            },
                        }, windowClass: 'modalDialog'
                    };
                    customerProxy.queryObject({ num: $scope.cust.customerNum, siteUseId: $scope.cust.siteUseId }, function (entity) {
                        var deal = entity.deal;
                        var modalDefaults = {
                            templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                            controller: 'contactorEditCtrl',
                            size: 'lg',
                            resolve: {
                                cont: function () {
                                    return new contactProxy();
                                },
                                num: function () {
                                    return $scope.cust.customerNum + ',' + $scope.cust.siteUseId;
                                },
                                site: function () {
                                    return deal;
                                },
                                langs: function () {
                                    return $scope.languagelist;
                                },
                                isHoldStatus: function () {
                                    return $scope.inUselist;
                                },
                                legal: function () {
                                    return $scope.legallist;
                                }
                            }, windowClass: 'modalDialog'
                        };

                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            contactProxy.forCustomer($scope.cust.customerNum + ',' + $scope.cust.siteUseId, function (contactlist) {
                                $scope.conlist = contactlist;
                            });

                        });
                    })
                } else {
                    alert("Please Create Customer First");
                }
            };

            //复制添加新Contacter
            $scope.CopyContacterInfo = function () {
                if ($scope.addFlg == 1 || custNum != 'newCust') {
  
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/contactor/contactor-copy.tpl.html',
                        controller: 'contactorCopyCtrl',
                        size: 'lg',
                        resolve: {
                            customerNum: function () {
                                return $scope.cust.customerNum;
                            },
                            siteUseId: function () {
                                return $scope.cust.siteUseId;
                            },
                            legal: function () {
                                $scope.cust.organization;
                            }
                        }, windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        contactProxy.forCustomer($scope.cust.customerNum + ',' + $scope.cust.siteUseId, function (contactlist) {
                            $scope.conlist = contactlist;
                        });
                    });
                } else {
                    alert("Please Create Customer First");
                }
            };


            $scope.AddAccountPeriod = function () {
                var rest = $scope.getUnAddLegal;
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    if ($scope.dunninglist.length != $scope.legallist.length) {
                        var modalDefaults = {
                            templateUrl: 'app/masterdata/customerAccountPeriod/accountPeriod.tpl.html',
                            controller: 'accountPeriodCtrl',
                            size: 'lg',
                            resolve: {
                                cont: function () {
                                    return '';
                                },
                                ap: function () {
                                    var ap = custWithGroup[0];
                                    return ap;
                                },
                                num: function () {
                                    return $scope.cust.customerNum;
                                },
                                legal: rest,
                                typeDetailList: function () {
                                    return $scope.typeDetailList;
                                },
                            }, windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            var num = $scope.cust.customerNum;
                            //获取账期年月
                            customerAccountPeriodProxy.getByNumAndSiteUseId(par, function (list) {
                                $scope.accountPeriodList.data = list;
                            }, function (res) {
                                alert(res);
                            })
                        });
                    } else {
                        alert("All Legal Entity have already add!");
                    }
                } else {
                    alert("Please Create Customer First");
                }
            };

            $scope.EditContacterInfo = function (row) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/contactor/contactor-edit.tpl.html',
                    controller: 'contactorEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        num: function () {
                            return $scope.cust.customerNum + ',' + $scope.cust.siteUseId;
                        },
                        site: function () {
                            return $scope.cust.deal;
                        },
                        langs: function () {
                            return $scope.languagelist;
                        },
                        isHoldStatus: function () {
                            return $scope.inUselist;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    contactProxy.forCustomer($scope.cust.customerNum + ',' + $scope.cust.siteUseId, function (contactlist) {
                        $scope.conlist = contactlist;
                    });
                });
            };

            $scope.Delcontacter = function (entity) {
                var cusid = entity.id;
                contactProxy.delContactor(cusid, function () {
                    alert("Delete Success");
                    contactProxy.forCustomer($scope.cust.customerNum + ',' + $scope.cust.siteUseId, function (contactlist) {
                        $scope.conlist = contactlist;
                    });
                }, function () {
                    alert("Delete Error");
                });
            };

            $scope.DelAccountPeriod = function (entity) {
                var apId = entity.id;
                customerAccountPeriodProxy.delAccountPeriod(apId, function () {
                    alert("Delete Success");
                    var num = $scope.cust.customerNum;
                    //获取账期年月
                    customerAccountPeriodProxy.getByNumAndSiteUseId(par, function (list) {
                        $scope.accountPeriodList.data = list;
                    }, function (res) {
                        alert(res);
                    })
                }, function () {
                    alert("Delete Error");
                });
            };


            //------------------------Comments-------------------------
            customerProxy.getComments($scope.cust.customerNum + ',' + $scope.cust.siteUseId, function (contactlist) {
                //console.log(contactlist);
                $scope.commlist = contactlist;
            });
            $scope.DelComments = function (entity) {
                var apId = entity.id;
                customerProxy.delCustomerComments(apId, function () {
                    alert("Delete Success");
                    customerProxy.getComments(par.customer_num + ',' + par.siteUseId, function (contactlist) {
                        $scope.commlist = contactlist;
                    });
                    $window.location.reload();
                }, function () {
                    alert("Delete Error");
                });
            };

            //添加新Comments
            $scope.AddCommentsInfo = function () {
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    var num = custNum;
                    $scope.nmbNum = custNum;
                    var rest = $scope.getUnAddLegal;
                   
                    var deal = {};
                        var modalDefaults = {
                            templateUrl: 'app/masterdata/customer/comments-edit.tpl.html',
                            controller: 'commentsEditCtrl',
                            size: 'lg',
                            resolve: {
                                cont: function () {
                                    return {};
                                },
                                num: function () {
                                    return $scope.cust.customerNum + ',' + $scope.cust.siteUseId;
                                },
                                site: function () {
                                    return deal;
                                },
                                langs: function () {
                                    return $scope.languagelist;
                                },
                                isHoldStatus: function () {
                                    return $scope.inUselist;
                                },
                                legal: function () {
                                    return $scope.legallist;
                                },
                                overduelist: function () {
                                    return $scope.overduelist;
                                }
                            }, windowClass: 'modalDialog'
                        };

                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            customerProxy.getComments($scope.cust.customerNum + ',' + $scope.cust.siteUseId, function (contactlist) {
                                $scope.commlist = contactlist;
                                $window.location.reload();
                            });

                        });
                } else {
                    alert("Please Create Customer First");
                }
            };

            $scope.EditCommentsInfo = function (row) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customer/comments-edit.tpl.html',
                    controller: 'commentsEditCtrl',
                    size: 'lg',
                    resolve: {
                        cont: function () {
                            return row;
                        },
                        num: function () {
                            return $scope.cust.customerNum + ',' + $scope.cust.siteUseId;
                        },
                        site: function () {
                            return $scope.cust.deal;
                        },
                        langs: function () {
                            return $scope.languagelist;
                        },
                        isHoldStatus: function () {
                            return $scope.inUselist;
                        },
                        legal: function () {
                            return $scope.legallist;
                        },
                        overduelist: function () {
                            return $scope.overduelist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    if (result != 'cancel') {
                        customerProxy.getComments($scope.cust.customerNum + ',' + $scope.cust.siteUseId, function (contactlist) {
                            $scope.commlist = contactlist;
                            $window.location.reload();
                        });
                    }
                });
            };

            $scope.commentsList = {
                data: 'commlist',
                multiSelect: false,
                columnDefs: [
                    { field: 'agingBucket', displayName: 'AgingBucket', width: '100' },
                    { field: 'ptpAmount', displayName: 'PTPAmount', width: '100', cellFilter: 'number:2', type: 'number', cellClass: 'right' },
                    { field: 'ptpdate', displayName: 'PTPDATE', cellFilter: 'date:\'yyyy-MM-dd\'', width: '80', cellClass: 'center' },
                    { field: 'overdueReason', displayName: 'OverdueReason', width: '200'},
                    { field: 'comments', displayName: 'Comments', width: '200' },
                    { field: 'commentsFrom', displayName: 'Source', width: '90' },
                    { field: 'createUser', displayName: 'CreateUser', width: '80', cellClass: 'center' },
                    { field: 'createDate', displayName: 'CreateDate', cellFilter: 'date:\'yyyy-MM-dd HH:mm:ss\'', width: '120', cellClass: 'center' },
                    {
                        name: 'o', displayName: 'Operation', width: '90',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditCommentsInfo(row.entity)"  class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelComments(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                    }
                ]
            };


            //end--------------------------Comments----------------
            $scope.contactList = {
                data: 'conlist',
                multiSelect: false,
                columnDefs: [
                    { field: 'name', displayName: 'Contact Name', width: '110' },
                    { field: 'emailAddress', displayName: 'Email', width: '180' },
                    { field: 'department', displayName: 'Department', width: '100' },
                    { field: 'title', displayName: 'Title' },
                    { field: 'number', displayName: 'Contact Number', width: '120' },
                    { field: 'comment', displayName: 'Comment', width: '90' },
                    {
                        field: 'communicationLanguage', displayName: 'language', width: '85',
                        cellTemplate: '<div hello="{valueMember: \'communicationLanguage\', basedata: \'grid.appScope.languagelist\'}"></div>'
                    },
                    {
                        field: 'groupCode', displayName: 'To/Cc',
                        cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckType(row.entity)}}</div>'
                    },
                    {
                        field: 'isGroupLevel', displayName: 'Is Group Level', width: '135',
                        cellTemplate: '<div style="margin-top:5px;margin-left:5px;">{{grid.appScope.CheckGroupLevel(row.entity)}}</div>', width: '90'
                    },
                    {
                        name: 'o', displayName: 'Operation', width: '90',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditContacterInfo(row.entity)"  class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.Delcontacter(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                    },

                ]
            };

            /*****************************************Payment Bank*****************************************************/

            $scope.AddBankInfo = function () {
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    var modalDefaults = {
                        templateUrl: 'app/masterdata/paymentbank/paymentbank-edit.tpl.html',
                        controller: 'paymentbankEditCtrl',
                        size: 'lg',
                        resolve: {
                            custInfo: function () {
                                return customerPaymentbankProxy.queryObject({ type: "new" });
                            }, num: function () {
                                return $scope.cust.customerNum;
                            }, flg: function () {
                                return $scope.inUselist;
                            },
                            legal: function () {
                                return $scope.legallist;
                            }
                        }, windowClass: 'modalDialog'
                    };

                    modalService.showModal(modalDefaults, {}).then(function (result) {
                        var num = $scope.cust.customerNum;
                        customerPaymentbankProxy.forCustBank(num, function (custbanklist) {
                            $scope.banklist = custbanklist;
                        });
                    });
                } else {
                    alert("Please Create Customer First");
                }
            };

            $scope.EditBankInfo = function (row) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/paymentbank/paymentbank-edit.tpl.html',
                    controller: 'paymentbankEditCtrl',
                    size: 'lg',
                    resolve: {
                        custInfo: function () {
                            return row;
                        }, num: function () {
                            return $scope.cust.customerNum;
                        }, flg: function () {
                            return $scope.inUselist;
                        },
                        legal: function () {
                            return $scope.legallist;
                        }
                    }, windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {
                    var num = $scope.cust.customerNum;
                    customerPaymentbankProxy.forCustBank(num, function (custbanklist) {
                        $scope.banklist = custbanklist;
                    });
                });
            };

            $scope.DelBankInfo = function (entity) {
                var cusid = entity.id;
                customerPaymentbankProxy.delPaymentBank(cusid, function () {
                    alert("Delete Success");
                    var num = $scope.cust.customerNum;
                    customerPaymentbankProxy.forCustBank(num, function (custbanklist) {
                        $scope.banklist = custbanklist;
                    });
                }, function () {
                    alert("Delete Error");
                });
            };

            $scope.bankList = {
                data: 'banklist',
                multiSelect: false,
                columnDefs: [
                    { field: 'bankAccountName', displayName: 'Account Name' },
                    { field: 'legalEntity', displayName: 'Legal Entity' },
                    { field: 'bankName', displayName: 'Bank Name' },
                    { field: 'bankAccount', displayName: 'Bank Account' },
                    { field: 'createDate', displayName: 'Create Date' },
                    { field: 'createPersonId', displayName: 'Create Person' },
                    {
                        field: 'flg', displayName: 'Status',
                        cellTemplate: '<div hello="{valueMember: \'flg\', basedata: \'grid.appScope.inUselist\'}"></div>'
                    },
                    { field: 'description', displayName: 'Description' },
                    {
                        name: 'o', displayName: 'Operation', width: '90',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditBankInfo(row.entity)" class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelBankInfo(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                    }
                ]
            };

            /****************************************Payment Circle*******************************************/
            $scope.addPaymentcircle = function () {
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    if (($scope.paymentCircle == null || $scope.paymentCircle == "")
                        && ($scope.reconciliationDay == null || $scope.reconciliationDay == "")) {
                        alert("Reconciliation Day or Payment Day have to choose one!");
                    }
                    else {
                        //if ($scope.entityFlg != null) {
                        var params = $routeParams.num.split(',');
                        var paymentCircle = [];

                        paymentCircle.push($scope.paymentCircle);
                        paymentCircle.push($scope.cust.customerNum);
                        paymentCircle.push($scope.entityFlg);
                        paymentCircle.push($scope.cust.siteUseId);
                        paymentCircle.push($scope.reconciliationDay);
                        customerPaymentcircleProxy.addPaymentCircle(paymentCircle, function (res) {
                            $scope.changeLegal($scope.entityFlg);
                            alert(res);
                        }, function (res) {
                            alert(res);
                        });
                        //} else {
                        //    alert("Please Select Legal Entity First!");
                        //}
                    }
                } else {
                    alert("Please Create Customer First");
                }
            };

            $scope.delAllPaymentcircle = function () {
                customerPaymentcircleProxy.delAllPaymentcircle($scope.cust.customerNum, $scope.cust.siteUseId, function (res) {
                    $scope.changeLegal($scope.entityFlg);
                    alert(res);
                }, function (res) {
                    alert(res);
                });
            };

            //初始化uploader

            var uploader = $scope.uploader = new FileUploader({
                url: APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle'
            });

            uploader.filters.push({
                name: 'customFilter',
                fn: function (item /*{File|FileLikeObject}*/, options) {
                    return this.queue.length < 10;
                }
            });

            // CALLBACKS
            uploader.onSuccessItem = function (fileItem, response, status, headers) {
                alert(response);
                var num = $scope.cust.customerNum;
                $scope.changeLegal($scope.entityFlg);
            };
            uploader.onErrorItem = function (fileItem, response, status, headers) {
                alert(response);
            };

            $scope.addFileCircle = function () {
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    if (uploader.queue[3] == null) {
                        alert("Please Select File");
                    } else {
                        if ((uploader.queue[3]._file.name.toString().toUpperCase().split(".").length > 1)
                            && uploader.queue[3]._file.name.toString().toUpperCase().split(".")[1] != "CSV") {
                            alert("File format is not correct !");
                            return;
                        }
                        var num = $scope.cust.customerNum;
                        var siteUseId = $scope.cust.siteUseId;
                        //var legal = $scope.entityFlg;
                        var legal = '';
                        uploader.queue[3].url = APPSETTING['serverUrl'] + '/api/CustomerPaymentcircle?customerNum=' + num + '&siteUseId=' + siteUseId + '&legal=' + legal;
                        uploader.uploadAll();
                        $scope.changeLegal($scope.entityFlg);
                    }
                } else {
                    alert("Please Create Customer First");
                }
            }

            $scope.DelPaymentcircle = function (entity) {
                var cusid = entity.id;
                customerPaymentcircleProxy.delPaymentCircle(cusid, function () {
                    alert("Delete Success");
                    var num = $scope.cust.customerNum;
                    $scope.changeLegal($scope.entityFlg);
                }, function () {
                    alert("Delete Error");
                });
            };

            $scope.paydayList = {
                data: 'paydaylist',
                multiSelect: false,
                //  enableFullRowSelection: true,
                columnDefs: [
                    { field: 'sortId', displayName: '#', width: '60' },
                    //{ field: 'legalEntity', displayName: 'Legal Entity' },
                    //{ field: 'paymentDay', displayName: 'Payment Day', cellFilter: 'date:\'yyyy-MM-dd\'' },
                    {
                        field: 'reconciliation_Day', displayName: 'Reconciliation DAY', cellFilter: 'date:\'yyyy-MM-dd\'',
                        width: '130'
                    },
                    //{ field: 'paymentWeek', displayName: 'Week Day',
                    //    cellTemplate: '<div hello="{valueMember: \'paymentWeek\', basedata: \'grid.appScope.weeklist\'}"></div>'
                    //},
                    //{ field: 'weekDay', displayName: 'Week Day' },
                    {
                        name: 'o', displayName: 'Operation', width: '90',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.DelPaymentcircle(row.entity)" class="glyphicon glyphicon-trash"></a></div>'
                    }
                ]
            };

            $scope.changeLegal = function (legal) {
                customerPaymentcircleProxy.searchPaymentCircle(
                    $scope.cust.customerNum + ',' + $scope.cust.siteUseId, legal
                    , function (paydate) {
                        $scope.paydaylist = paydate;
                        uploader.queue[3] = null;
                        document.getElementById("uploadCalendar").value = "";
                    },
                    function () {
                    });
            }

            //*************************** Dunning*************************************************
            $scope.dunningList = {
                data: 'dunninglist',
                multiSelect: false,
                columnDefs: [
                    { field: 'legalEntity', displayName: 'Legal Entity', width: '100' },
                    //{ field: 'riskInterval', displayName: 'Risk Interval', width: '120' },
                    { field: 'description', displayName: 'Description', width: '110' },
                    {
                        name: 'o', displayName: 'Operation', width: '90',
                        cellTemplate: '<div class="ngCellText" ng-class="col.colIndex()">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a ng-click="grid.appScope.EditConfig(row.entity)" class="glyphicon glyphicon-pencil"></a>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</a></div>'
                    },
                    {
                        field: 'AutoSendConfirmPTP', displayName: 'AutoSendConfirmPTP', width: '150',
                        cellTemplate: '<input type="checkbox" name="AutoSendReminder" ng-checked="row.entity.autoSendConfirmPTP" />'
                    },

                    {
                        field: 'AutoSendReminder', displayName: 'AutoSendReminder', width: '150',
                        cellTemplate: '<input type="checkbox" name="AutoSendReminder" ng-checked="row.entity.autoSendReminder" />'
                    }
                    //{
                    //    field: 'AutoSendDunning', displayName: 'AutoSendDunning', width: '150',
                    //    cellTemplate: '<input type="checkbox" name="AutoSendDunning" ng-checked="row.entity.autoSendDunning" />'
                    //}
                    //{ field: 'confirm1Days', displayName: 'FirstTimeConfirmStartDate' , width:'170'},
                    //{ field: 'confirm1CommunicationMethod', displayName: 'ConfirmMethod', width: '110' },
                    //{ field: 'confirm2Days', displayName: 'SecondTimeConfirmStartDate', width:'180' },
                    //{ field: 'confirm2CommunicationMethod', displayName: 'ConfirmMethod', width: '110' },
                    //{ field: 'remindingDays', displayName: 'RemindingStartDate', width: '140'},
                    //{ field: 'remindingCommunicationMethod', displayName: 'RemindMethod', width: '110' },
                    //{ field: 'dunning1Days', displayName: 'FirstTimeDunningStartDate', width: '170'},
                    //{ field: 'dunning1CommunicationMethod', displayName: 'DunMethod', width: '110' },
                    //{ field: 'dunning2Days', displayName: 'SecondTimeDunningStartDate', width: '180' },
                    //{ field: 'dunning2CommunicationMethod', displayName: 'DunMethod', width: '110'  }
                ]
            };

            dunningProxy.forConfig(custNum, function (config) {
                $scope.dunninglist = config;
            })

            $scope.getUnAddLegal = function () {
                var restlist = [];
                if ($scope.dunninglist.length != 0) {
                    angular.forEach($scope.legallist, function (row1) {
                        var k = 0;
                        angular.forEach($scope.dunninglist, function (row2) {
                            if (row1.legalEntity == row2.legalEntity) {
                                k = 1;
                            }
                        })
                        if (k == 0) {
                            restlist.push(row1);
                        }
                    })
                    return restlist;
                } else {
                    return $scope.legallist;
                }
            }

            $scope.AddConfig = function () {
                var rest = $scope.getUnAddLegal;
                if ($scope.addFlg == 1 || custNum != 'newCust') {
                    if ($scope.dunninglist.length != $scope.legallist.length) {
                        var modalDefaults = {
                            templateUrl: 'app/masterdata/dunning/dunning-edit.tpl.html',
                            controller: 'dunningEditCtrl',
                            size: 'lg',
                            resolve: {
                                config: function () {
                                    return new dunningProxy();
                                },
                                num: function () {
                                    return $scope.cust.customerNum;
                                },
                                siteUseId: function () {
                                    return $scope.cust.siteUseId;
                                },
                                legal: rest,
                                typeDetailList: function () {
                                    return $scope.typeDetailList;
                                },
                            }, windowClass: 'modalDialog'
                        };
                        modalService.showModal(modalDefaults, {}).then(function (result) {
                            var num = $routeParams.num;
                            dunningProxy.forConfig(num, function (config) {
                                $scope.dunninglist = config;
                            })
                        });
                    } else {
                        alert("All Legal Entity have already add!");
                    }
                } else {
                    alert("Please Create Customer First");
                }
            };

            $scope.EditConfig = function (row) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/dunning/dunning-edit.tpl.html',
                    controller: 'dunningEditCtrl',
                    size: 'lg',
                    resolve: {
                        config: function () {
                            return row;
                        },
                        num: function () {
                            return $scope.cust.customerNum;
                        },
                        siteUseId: function () {
                            return $scope.cust.siteUseId;
                        },
                        legal: function () {
                            var currlegallist = [];
                            for (var i = 0; $scope.legallist.length; i++)
                                if ($scope.legallist[i].legalEntity == row.legalEntity) {
                                    currlegallist.push($scope.legallist[i])
                                    return currlegallist;
                                }
                        },
                        typeDetailList: function () {
                            return $scope.typeDetailList;
                        },
                    }, windowClass: 'modalDialog'
                };
                modalService.showModal(modalDefaults, {}).then(function (result) {
                    var num = $routeParams.num;
                    dunningProxy.forConfig(num, function (config) {
                        $scope.dunninglist = config;
                    })
                });
            };

            $scope.openMemmoDateHis = function (customerCode, siteUseId) {
                var modalDefaults = {
                    templateUrl: 'app/masterdata/customer/CustomerExpirationDate.tpl.html',
                    controller: 'customerExpirationDateCTL',
                    size: 'lg',
                    resolve: {
                        customerCode: function () { return customerCode; },
                        siteUseId: function () { return siteUseId; }
                    }
                    , windowClass: 'modalDialog'
                };

                modalService.showModal(modalDefaults, {}).then(function (result) {

                });

            }
            //******************************************************************************************
        }]);

