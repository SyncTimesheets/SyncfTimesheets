var employeeList = [{ name: "ananth.manickam" }, { name: "narayanasamyj" }, { name: "venu.perumal" }, {name:"tamil"}];

$(document).ready(function () {
    $("#spinner-overlay").addClass("hide").removeClass("show");



    $("#intializeFilter").on("click", function (e) {
        $("#loadingmodal").modal("show");
        setTimeout(function () {
            var filterString = "";
            var dateRangeObject = $("#daterange").data("ejDateRangePicker");
            var employeeListObject = $("#employeeList").data("ejAutocomplete");
            var selectedEmployee = employeeListObject.getValue();
            var dateRange = dateRangeObject.getSelectedRange();
            if (!jQuery.isEmptyObject(dateRange)) {
                if (!selectedEmployee) {
                    filterString = "worklogDate >= " + dateRange.startDate.getFullYear() + "-" + (dateRange.startDate.getMonth() + 1) + "-" + dateRange.startDate.getDate() + " and worklogDate <= " + dateRange.endDate.getFullYear() + "-" + (dateRange.endDate.getMonth() + 1) + "-" + dateRange.endDate.getDate() + " and worklogAuthor in (" + sessionStorage.getItem("UserName") + ")";
                }
                else {
                    selectedEmployee = selectedEmployee.substring(0, selectedEmployee.length - 1);
                    selectedEmployee = selectedEmployee.split(",");
                    var emplooyee = null;
                    var employees = $.each(selectedEmployee, function (index, name) {
                        if (!emplooyee) {
                            emplooyee = "'" + name + "'";
                        } else {
                            emplooyee = emplooyee + ",'" + name + "'";
                        }
                    });
                    emplooyees = emplooyee;
                    filterString = "worklogDate >= " + dateRange.startDate.getFullYear() + "-" + (dateRange.startDate.getMonth() + 1) + "-" + dateRange.startDate.getDate() + " and worklogDate <= " + dateRange.endDate.getFullYear() + "-" + (dateRange.endDate.getMonth() + 1) + "-" + dateRange.endDate.getDate() + " and worklogAuthor in (" + emplooyees + ")";
                }
            }
            if (filterString) {
                $.ajax({
                    async: false,
                    type: "POST",
                    url: "/HOME/FilterDateRange",
                    data: { query: filterString },
                    success: function (data) {
                        if (data.TaskList.length != null && data.TaskList.length != 0) {
                            $("#timesheet-subscribe").removeClass("hide");
                        }
                        loadGrid(dateRange, data);
                        var gridObj = $("#Grid").data("ejGrid");
                        gridObj.groupColumn("JIRAId");
                        gridObj.collapseAll();
                        $("#loadingmodal").modal("hide");
                    }
                });
            }
            $("#loadingmodal").modal("hide");
        }, 500);                
    });

    $("#signout").hide();
    
    //if (!userCrenditial.userName && !userCrenditial.passWord) {
    //    $("#LoginModal").modal("show");
    //}
    
    $('#login').on("click", function (e) {
        $("#loadingmodal").modal("show");
        setTimeout(function () {
            var userName = $("#userID").val();
            if (userName.indexOf("@") > 0) {
                userName = userName.substring(0, userName.indexOf("@"));
            }
            var password = $("#userPassword").val();
            $.ajax({
                async: false,
                type: "POST",
                url: "/HOME/checkCrendintail",
                data: { UserName: userName, Password: password },
                success: function (data) {
                    if (data.toLowerCase() == "true") {
                        sessionStorage["UserName"] = userName;
                        loginSucess();
                        $("#loadingmodal").modal("hide");
                        window.location.href = "Home/Timesheet";
                    }
                    else {
                        $("#userId, #userPassword").empty();
                        $("#loadingmodal").modal("hide");
                    }
                }
            });
            $("#loadingmodal").modal("hide");
        }, 500);        
    });

    $("#timesheet-subscribe").on("click", function () {
        $.ajax({
            async: false,
            type: "POST",
            url: "/HOME/subscribe",          
            success: function (data) {
                if (data.toLowerCase() == "true") {
                    console.log(data);
                }
                else {
                   console.log(data);
                }
            }
        });
    });

    function loginSucess() {
        $("#signout").show();       
        $("#UserName").append("<a>" + sessionStorage.getItem("UserName")+ "</a>");
    }
     
});


function loadGrid(dateRange, data) {

    var stackedRows = [{ stackedHeaderColumns: [{ headerText: "Time Sheet Period", column: "EngineerName,projectName,JIRAId" }] }];
    var summaryRows = [{ showCaptionSummary: true, summaryColumns: [], showTotalSummary: true }];
    var columns = [{ field: "EngineerName", headerText: "Time Entry User", width: 120, textAlign: ej.TextAlign.Center },
    { field: "projectName", headerText: "Project Name", template: "#projectNameColumnTemplate", width: 120, textAlign: ej.TextAlign.Center },
    { field: "JIRAId", headerText: "Key", template: "#jiraIdColumnTemplate", width: 120, textAlign: ej.TextAlign.Center }];

    var startDate = new Date(dateRange.startDate);
    var endDate = new Date(dateRange.endDate);
    for (var d = startDate ; d <= endDate; d.setDate(d.getDate() + 1)) {
        var field = moment(d).format("DDMMMYYYY");
        var stackedHeaderText = moment(d).format("MMM") + " " + moment(d).format("DD");
        var day = moment(d).format("dd");
        var col = { field: field, headerText: day, width: 55, textAlign: ej.TextAlign.Center, defaultValue: "-" };
        var stackedColumns = { headerText: stackedHeaderText, column: field };
        var summaryColumn = { summaryType: ej.Grid.SummaryType.Sum, displayColumn: field, dataMember: field, template: "#summaryTemplate" };
        columns.push(col);
        stackedRows[0].stackedHeaderColumns.push(stackedColumns);
        summaryRows[0].summaryColumns.push(summaryColumn);

    }

    var dataSource = data.TaskList;

    for (i = 0; i < dataSource.length; i++) {
        var logDate = moment(dataSource[i].worklogDate);
        var colName = logDate.format("DDMMMYYYY");
        dataSource[i][colName] = dataSource[i].loghours;
        dataSource[i]["ProjectNameLink"] = encodeURI("https://syncfusion.atlassian.net/issues/?jql=project=" + "'" + dataSource[i].projectName + "'");
        dataSource[i]["JiraIdLink"] = "https://syncfusion.atlassian.net/browse/" + dataSource[i].JIRAId;
    }
    $("#Grid").ejGrid({
        dataSource: dataSource,
        allowPaging: false,
        allowSorting: true,
        allowGrouping: true,
        allowScrolling: true,
        isResponsive: true,
        showStackedHeader: true,
        showSummary: true,
        stackedHeaderRows: stackedRows,
        scrollSettings: { width: "100%" },
        summaryRows: summaryRows,
        groupSettings: { groupedColumns: ["JIRAId"] },
        columns: columns,
        dataBound: function (args) {
            this.collapseAll();
        },
        queryCellInfo: function (args) {
            var value = args.text.replace(",", "");
            var $element = $(args.cell);
            debugger
            if (!value)
                $element.html("-");
        }
    });
}

