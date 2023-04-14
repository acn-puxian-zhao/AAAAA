angular.module('app.dashboard', [])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider

            .when('/dashboard', {
                templateUrl: 'app/common/report/dashboard.tpl.html',
                controller: 'dashboardCtrl',
                resolve: {

                }
            });
    }])

    //*****************************************header***************************s
    .controller('dashboardCtrl', ['$scope', 'periodProxy', '$http', 'modalService', 'dashboardProxy', 'uiGridConstants',
        function ($scope, periodProxy, $http, modalService, dashboardProxy, uiGridConstants) {
            $scope.$parent.helloAngular = "OTC - Dashboard";

            dashboardProxy.loadReport(function (json) {
                $scope.TotalAMT = json.totalAMT;
                $scope.ConfirmTotal = json.confirmTotal;
                $scope.OverdueTotal = json.overdueTotal;
                $scope.DisputeTotal = json.disputeTotal;
                if (json.noCollector !== 0)
                {
                    $scope.noCollector = json.noCollector;
                    $scope.noCollectorperfix = "Miss Collector: ";
                }
                else
                {
                    $scope.noCollector = "";
                    $scope.noCollectorperfix = "";
                }
                if (json.noUpload !== "")
                {
                    $scope.noUpload =json.noUpload;
                    $scope.noUploadperfix ="Miss AR Report: ";
                }
                else
                {
                    $scope.noUpload = "";
                    $scope.noUploadperfix = "";
                }

                // 仪表盘
                var confirmChart = echarts.init(document.getElementById('confirmPTP'), 'macarons');
                var overdueChart = echarts.init(document.getElementById('overdue'), 'macarons');
                var disputeChart = echarts.init(document.getElementById('dispute'), 'macarons');

                option = {
                    toolbox: {
                        show: false,
                        feature: {
                            mark: { show: true },
                            restore: { show: true },
                            saveAsImage: { show: true }
                        }
                    },
                    ribbonType: false,
                    clickable: true,
                    draggable: false,
                    series: [
                        {
                            type: 'gauge',
                            detail: { formatter: '{value}%' },
                            data: [{ value: 50 }],
                            clickable: false
                        }
                    ]
                };

                option.series[0].data[0].value = (($scope.ConfirmTotal / $scope.TotalAMT) * 100).toFixed(0);
                confirmChart.setOption(option, true);

                option.series[0].data[0].value = (($scope.OverdueTotal / $scope.TotalAMT) * 100).toFixed(0);
                overdueChart.setOption(option, true);

                option.series[0].data[0].value = (($scope.DisputeTotal / $scope.TotalAMT) * 100).toFixed(0);
                disputeChart.setOption(option, true);

                //柱状图

                // 指定图表的配置项和数据
                var reasonChartOption = {
                    title: {
                        text: 'OverdueReason'
                    },
                    tooltip: {
                        trigger: 'axis'
                    },
                    legend: {
                        data: ['逾期金额']
                    },
                    toolbox: {
                        show: false,
                    },
                    calculable: true,
                    xAxis: [
                        {
                            type: 'category',
                            data: (function () {
                                var d = [];
                                for (let item of json.overdueReasonStatistics) {
                                    d.push(item.itemName);
                                }
                                return d;
                            })(),
                            axisLabel: {
                                interval: 0,
                                rotate: 25
                            }
                        }
                    ],
                    yAxis: [
                        {
                            type: 'value'
                        }
                    ],
                    series: [
                        {
                            name: '逾期金额',
                            type: 'bar',
                            data: (function () {
                                var d = [];
                                for (let item of json.overdueReasonStatistics) {
                                    d.push(item.amt);
                                }
                                return d;
                            })(),
                            markPoint: {
                                data: [
                                    { type: 'max', name: '最大值' },
                                    { type: 'min', name: '最小值' }
                                ]
                            }
                        }
                    ]
                };

                var reasonChart = echarts.init(document.getElementById('OverdueReason'));
                // 使用刚指定的配置项和数据显示图表。
                reasonChart.setOption(reasonChartOption);
               

                // 指定图表的配置项和数据
                var agingChartOption = {
                    title: {
                        text: 'Overdue By Aging Bucket'
                    },
                    tooltip: {
                        trigger: 'axis'
                    },
                    legend: {
                        data: ['Aging']
                    },
                    toolbox: {
                        show: false,
                    },
                    calculable: true,
                    xAxis: [
                        {
                            type: 'category',
                            data: (function () {
                                var d = [];
                                for (let item of json.overdueAgingStatistics) {
                                    d.push(item.itemName);
                                }
                                return d;
                            })(),
                            axisLabel: {
                                interval: 0,
                                rotate: 25
                            }
                        }
                    ],
                    yAxis: [
                        {
                            type: 'value'
                        }
                    ],
                    series: [
                        {
                            name: 'Aging Bucket',
                            type: 'bar',
                            data: (function () {
                                var d = [];
                                for (let item of json.overdueAgingStatistics) {
                                    d.push(item.amt);
                                }
                                return d;
                            })(),
                            markPoint: {
                                data: [
                                    { type: 'max', name: '最大值' },
                                    { type: 'min', name: '最小值' }
                                ]
                            }
                        }
                    ]
                };
                // 使用刚指定的配置项和数据显示图表。
                var agingChart = echarts.init(document.getElementById('OverdueAging'));
                agingChart.setOption(agingChartOption);
            });
            $scope.ChartClick = function (event) {
                //window.location.href = event.currentTarget.dataset.targeturl;
            };
        }])
