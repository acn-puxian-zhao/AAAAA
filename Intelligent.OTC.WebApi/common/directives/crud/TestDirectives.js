angular.module('directives.test', [])

// Apply this directive to an element at or below a form that will manage CRUD operations on a resource.
// - The resource must expose the following instance methods: $saveOrUpdate(), $id() and $remove()
.directive('helloWorld', function () {
    return {
        scope: true,
        require: '^form',
        link: function (scope, element, attrs) {
            scope.cb = function () {
                return true;
            };
        }
    };
});