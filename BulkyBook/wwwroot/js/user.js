﻿var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/User/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "company.name", "width": "15%" },
            { "data": "role", "width": "15%" },
            {
                "data": {id:"id", lockoutEnd:"lockoutEnd"},
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        // user is currently locked

                        return `
                            <div class="text-center">
                                
                                <a onClick=LockUnlock("${data.id}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-lock-open mr-1"></i> Unlock
                                </a>
                            </div>
                           `;
                    } else {
                        // user is currently unlocked

                        return `
                            <div class="text-center">
                                
                                <a onClick=LockUnlock("${data.id}") class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-lock"></i> Lock
                                </a>
                            </div>
                           `;
                    }
                    
                }, "width": "25%"
            }
        ]
    });
}
