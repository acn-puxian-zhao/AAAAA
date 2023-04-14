var DemoController = function ($scope, $resource) {

    var userResource = $resource('/api/book', {}, { update: { method: 'PUT'} });
    $scope.usersList = [
    { "id": 1, "firstName": "John", "lastName": "Smith", "gender": 0, "mobile": "9999999991", "email": "john@demo.com", "city": "kn", "state": "as", "country": "usa", "zip": "12401" },
    { "id": 2, "firstName": "Adam", "lastName": "Gril", "gender": 1, "mobile": "9999999992", "email": "adam@demo.com", "city": "bk", "state": "al", "country": "usa", "zip": "99701" },
    { "id": 3, "firstName": "James", "lastName": "Franklin", "gender": 0, "mobile": "9999999993", "email": "james@demo.com", "city": "js", "state": "nj", "country": "usa", "zip": "07097" },
    { "id": 4, "firstName": "Vicky", "lastName": "Merry", "gender": 1, "mobile": "9999999994", "email": "vicky@demo.com", "city": "ol", "state": "ny", "country": "usa", "zip": "14760" },
    { "id": 5, "firstName": "Cena", "lastName": "Rego", "gender": 0, "mobile": "9999999995", "email": "cena@demo.com", "city": "as", "state": "tx", "country": "usa", "zip": "78610" }
     ];

    //    userResource.query(function (data) {
    //        $scope.usersList = [];
    //        angular.forEach(data, function (userData) {
    //            $scope.usersList.push(userData);
    //        });
    //    });

    $scope.selectedUsers = [];

    $scope.$watchCollection('selectedUsers', function () {
        $scope.selectedUser = angular.copy($scope.selectedUsers[0]);
    });

    $scope.pagingOptions = {
        pageSizes: [250, 500, 1000],
        pageSize: 250,
        currentPage: 1
    };	

    $scope.userGrid = {
        data: 'usersList',
        jqueryUITheme: true, 
        multiSelect: false,
        showFooter: true,
        selectedItems: $scope.selectedUsers,
        enableColumnResize: false,
        columnDefs: [
            { field: 'firstName', displayName: 'First Name', width: '25%' },
            { field: 'lastName', displayName: 'Last Name', width: '25%' },
            { field: 'email', displayName: 'Email', width: '25%' },
            { field: 'mobile', displayName: 'Mobile Number', width: '25%' }
        ],
        pagingOptions: $scope.pagingOptions
    };

    $scope.updateUser = function (user) {
        userResource.update(user, function (updatedUser) {
            $scope.selectedUsers[0].id = updatedUser.id;
            $scope.selectedUsers[0].firstName = updatedUser.firstName;
            $scope.selectedUsers[0].lastName = updatedUser.lastName;
            $scope.selectedUsers[0].gender = updatedUser.gender;
            $scope.selectedUsers[0].mobile = updatedUser.mobile;
            $scope.selectedUsers[0].email = updatedUser.email;
            $scope.selectedUsers[0].city = updatedUser.city;
            $scope.selectedUsers[0].state = updatedUser.state;
            $scope.selectedUsers[0].country = updatedUser.country;
            $scope.selectedUsers[0].zip = updatedUser.zip;
        });
    };

    $scope.countryList = [
        {
            name: 'USA', id: 'usa', states: [
                { name: 'Alabama', id: 'al', cities: [{ name: 'Alabaster', id: 'al' }, { name: 'Arab', id: 'ar' }, { name: 'Banks', id: 'bk'}] },
                { name: 'Alaska', id: 'as', cities: [{ name: 'Lakes', id: 'lk' }, { name: 'Kenai', id: 'kn' }, { name: 'Gateway', id: 'gw'}] },
                { name: 'New Jersey', id: 'nj', cities: [{ name: 'Atlanta', id: 'at' }, { name: 'Jersey', id: 'js' }, { name: 'Newark', id: 'nw'}] },
                { name: 'New York', id: 'ny', cities: [{ name: 'Kingston', id: 'kg' }, { name: 'Lockport', id: 'lp' }, { name: 'Olean', id: 'ol'}] },
                { name: 'Texas', id: 'tx', cities: [{ name: 'Dallas', id: 'dl' }, { name: 'Austin', id: 'as' }, { name: 'Houston', id: 'hs'}]}]
        }
    ];

    $scope.clearCityAndZip = function () {
        $scope.selectedUsers[0].city = null;
        $scope.selectedUsers[0].zip = "";
    };

    $scope.$watch('selectedUsers[0].state', function (selectedStateId) {
        if (selectedStateId) {
            angular.forEach($scope.countryList[0].states, function (state) {
                if (selectedStateId == state.id) {
                    $scope.selectedState = state;
                }
            });
        }
    });

    var init = function () {

    }

    init();
};

// The $inject property of every controller (and pretty much every other type of object in Angular) needs to be a string array equal to the controllers arguments, only as strings
DemoController.$inject = ['$scope', '$resource'];


var DialogDemoController = function ($scope, $resource, $uibModal, modalService) {

    var userResource = $resource('/api/book', {}, { update: { method: 'PUT'} });
    $scope.usersList = [
    { "id": 1, "firstName": "John", "lastName": "Smith", "gender": 0, "mobile": "9999999991", "email": "john@demo.com", "city": "kn", "state": "as", "country": "usa", "zip": "12401" },
    { "id": 2, "firstName": "Adam", "lastName": "Gril", "gender": 1, "mobile": "9999999992", "email": "adam@demo.com", "city": "bk", "state": "al", "country": "usa", "zip": "99701" },
    { "id": 3, "firstName": "James", "lastName": "Franklin", "gender": 0, "mobile": "9999999993", "email": "james@demo.com", "city": "js", "state": "nj", "country": "usa", "zip": "07097" },
    { "id": 4, "firstName": "Vicky", "lastName": "Merry", "gender": 1, "mobile": "9999999994", "email": "vicky@demo.com", "city": "ol", "state": "ny", "country": "usa", "zip": "14760" },
    { "id": 5, "firstName": "Cena", "lastName": "Rego", "gender": 0, "mobile": "9999999995", "email": "cena@demo.com", "city": "as", "state": "tx", "country": "usa", "zip": "78610" }
     ];

    $scope.selectedUsers = [];

    $scope.$watchCollection('selectedUsers', function () {
        $scope.selectedUser = angular.copy($scope.selectedUsers[0]);

        //        $scope.selectedUser.prototype.$update = function(updatecb, errorUpdatecb){alert('update called');};
        //        $scope.selectedUser.prototype.$save = function (savecb, errorSavecb) { alert('save called'); };
        //        $scope.selectedUser.prototype.$saveOrUpdate = function (savecb, updatecb, errorSavecb, errorUpdatecb) {
        //            if (this.$id()) {
        //                return this.$update(updatecb, errorUpdatecb);
        //            } else {
        //                return this.$save(savecb, errorSavecb);
        //            }
        //        };
    });

    $scope.userGrid = {
        data: 'usersList',
        jqueryUITheme: true, 
        multiSelect: false,
        selectedItems: $scope.selectedUsers,
        enableColumnResize: false,
        columnDefs: [
            { field: 'firstName', displayName: 'First Name', width: '25%' },
            { field: 'lastName', displayName: 'Last Name', width: '25%' },
            { field: 'email', displayName: 'Email', width: '25%' },
            { field: 'mobile', displayName: 'Mobile Number', width: '25%',
                cellTemplate: '<div><input type="button" id="btnSubmit" class="btn btn-default" ng-click="openDialog(selectedUser)" value="EDIT" /></div>'
            }
        ]
    };

    $scope.openDialog = function () {
        // Inlined template for demo

        var modalDefaults = {
            templateUrl: 'app/views/Demo/userEditForm.html',
            controller: function ($scope) {
                $scope.closeButtonText= 'Cancel',
                $scope.actionButtonText= 'Commit',
                $scope.headerText= 'Edit Dialog'
                $scope.ok = function () {
                    alert('ok clicked');
                };                
                $scope.close = function () {
                    alert('close clicked');
                };
//                $scope.selectedUser = $scope.$parent.selectedUser;

//                $scope.selectedUser = [];
                $scope.selectedUser.firstName = 'name';
            }
        };

        var modalOptions = {
            selectedUser: $scope.selectedUser,
        };

        modalService.showModal(modalDefaults, modalOptions).then(function (result) {
            // something you want to do after commit.

        });
    }

};

// The $inject property of every controller (and pretty much every other type of object in Angular) needs to be a string array equal to the controllers arguments, only as strings
DialogDemoController.$inject = ['$scope', '$resource', '$uibModal', 'modalService'];

angular.module('ui.bootstrap.demo')
    .controller('DemoController', DemoController);