angular.module('app.common.paging', [])

    .controller('pagingCtrl', ['$scope', function ($scope) {

        $scope.Params = [];
        $scope.Params[1] = $scope.currentPage;
        $scope.Params[2] = $scope.itemsperpage;
        $scope.Params[3] = $scope.totalItems;

        //下拉单页容量初始化
        $scope.selectedLevel = 15;  
        //初始化单页容量
        $scope.itemsperpage = 15;
        //当前页
        $scope.currentPage = 1; 
        //分页显示的最大页     
        $scope.maxSize = 10; 
        //分页容量下拉列表定义
        $scope.levelList = [
            { "id": 15, "levelName": '15' },
            { "id": 500, "levelName": '500' },
            { "id": 1000, "levelName": '1000' },
            { "id": 2000, "levelName": '2000' },
            { "id": 5000, "levelName": '5000' },
            { "id": 999999, "levelName": 'ALL' }
        ];

        //单页容量变化
        $scope.pagesizechange = function () {
            $scope.itemsperpage = selectedLevelId;
            $scope.callback();
            $scope.calculate();
        };

        //翻页
        $scope.pageChanged = function () {
            $scope.callback();
            $scope.calculate();
        };

        //计算剩下页数
        $scope.calculate = function () {
            var currentPage = $scope.currentPage;
            var itemsperpage = $scope.itemsperpage;
            var count = 0;//$scope.params[0];

            if (count == 0) {
                $scope.fromItem = 0;
            } else {
                $scope.fromItem = (currentPage - 1) * itemsperpage + 1;
            }
            $scope.toItem = (currentPage - 1) * itemsperpage + count;
        }



    } ]);