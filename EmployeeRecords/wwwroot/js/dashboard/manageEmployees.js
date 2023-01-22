const apiUrl = "/api/employees/";
const label = "employees";
const itemsTable = $("#items");
const itemForm = $("#manageItems")[0];
let origForm = null;
var selectedDepartment = null;
var $ = window.$;

var query = {
	page: {
		number: 1,
		size: 10
	},
  sort: {
	  by: "name",
	  isAscending: true
  },
	withTotal: true,
	searchQuery: ""
};
var entries = {
  from: 0,
  to: 0,
  all: 0
};
var itemToDeleteId = null;

function AddRow(table, employee, isOdd) {
  if (isOdd == null)
	isOdd = $(table).find("tr:last").hasClass("odd") ? true : false; 
  let row = `<tr id='item_${employee.id}'`;
  if (!isOdd) row += 'class="odd"';
  row += `><td head='Name'>${employee.name}</td>
						<td head='Department' data-id="${employee.department.id}">${employee.department.name}</td>
						<td head='Date of birth'>${employee.dateOfBirth}</td>
						<td head='Address'>${employee.address}</td>
						
						<td head='Actions'>
						<a href="/employees/${employee.id}/files" target="_blank"><i class="fas fa-file-archive me-3 mb-3"></i></a>
						<a href="#" class="edit-btn" data-id="${employee.id}"><i class="fas fa-edit me-3 mb-3"></i></a>
						<a href="#" class="remove-btn" data-id="${employee.id}"><i class="fas fa-trash"></i></a>
						</td>
						</tr>`;
  $(table).find("tbody:eq(0)").append(row);
}

function ClearInputs() {
  $(itemForm)[0].reset();
  $("#Id").val(0);
  $("#DepartmentId").val("");
  $("#departmentName").typeahead("val", "");
  selectedDepartment = null;
  itemToDeleteId = null;

  const validator = $(itemForm).validate();

  const errors = $(itemForm).find(".field-validation-error span");
  errors.each(function() {
	   validator.settings.success($(this));
	   $(this).remove();
  });

  $(".field-validation-valid span").remove();
  validator.resetForm();
}

function ConfigureSubmit(id, table) {
  $(`#${id}`).submit(function (e) {
	e.preventDefault();

	const saveResource = getFormDataAsJson($(this));
	const submitType = $('#manageModal button[type="submit"]').text();
	SaveItem(saveResource, submitType, table);
  });
}

function ConfigureEdit() {
  $("#items").on("click", "a.edit-btn", function (e) {
	e.preventDefault();
	ClearInputs();
	const id = $(this).attr("data-id");
	const tableRow = $(`#item_${id}`);
	$("#Id").val(id);
	const  employeeName = tableRow.children(":eq(0)").text();
	$("#Name").val(employeeName);
	const departmentId = tableRow.children(":eq(1)").attr("data-id");
	selectedDepartment = {
		id: departmentId,
		name: tableRow.children(":eq(1)").text()
	};

	$("#DepartmentId").val(selectedDepartment.id);
	$("#departmentName").val(selectedDepartment.name);
	$("#DateOfBirth").val(tableRow.children(":eq(2)").text());
	$("#Address").val(tableRow.children(":eq(3)").text());
	$('#manageModal button[type="submit"]').text("Save");
	$("#manageModalLabel").text(`Edit emplyee: ${employeeName}`);
	$("#manageModal").modal("toggle");
  });
}

function ConfigureDelete(table, id) {
  $(table).on("click", "a.remove-btn", function (e) {
	e.preventDefault();
	id = $(this).attr("data-id");const tableRow = $(`#item_${id}`);
	const name = tableRow.children(":eq(0)").text();
	const department = tableRow.children(":eq(1)").text();
	const dateOfBirth = tableRow.children(":eq(2)").text();
	const address = tableRow.children(":eq(3)").text();

	$("#deleteParagraph").html(`<p>Are you sure you want to delete this employee?</p>
		<div class="col-md-6 col-8">
		<div>Name: ${name}</div><div>Department: ${department}</div>
		<div>Date of birth: ${dateOfBirth}</div><div>Address: ${address}</div>
		</div>`);
	$("#deleteModal").modal("toggle");
  });

  $("#deleteItem").submit(function (e) {
	e.preventDefault();
	DeleteItem(id, "Employee");
  });
}

function UpdateRow(id, employee) {
  const row = $(`${id}`);
  row.find("td").eq(0).text(employee.name);
  row.find("td").eq(1).text(employee.department.name);
  row.find("td").eq(1).attr("data-id", employee.department.id);
  row.find("td").eq(2).text(employee.dateOfBirth);
  row.find("td").eq(3).text(employee.address);
}

function ConvertSaveResourceToDisplayResource(saveResource, department, id) {
	const resource = {
		id: id, 
		name: saveResource.Name, 
		dateOfBirth: saveResource.DateOfBirth,
		address: saveResource.Address, 
		department: department
	};
	return resource;
}

function SaveItem(saveResource, saveType, table) {
	switch (saveType) {
	case "Save":
		var id = $("#Id").val();
		$.ajax({
			async: false,
			url: apiUrl + id,
			method: "PUT",
			contentType: "application/json",
			data: saveResource,
			statusCode: {
				400: function(response) {
					const data = JSON.parse(response.responseText);
					$("#manageItems").data("validator").showErrors(data);
				},
				404: function() {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("Employee not found.");
				},
				204: function() {
					
					const resource = ConvertSaveResourceToDisplayResource(
						JSON.parse(saveResource),
						selectedDepartment, id);
					UpdateRow(`#item_${id}`, resource);
					const message = "Changes have been saved successfully.";
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification(message);
				},
				500: function() {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("Internal server error, please try again.");
				}
			}
		});
		break;
	case "Add":
		$.ajax({
			async: false,
			url: apiUrl,
			method: "POST",
			contentType: "application/json",
			data: saveResource,
			statusCode: {
				201: function (result) {
					const resource = ConvertSaveResourceToDisplayResource(
						JSON.parse(saveResource),
						selectedDepartment, result.id);
					AddRow(table, resource, null);
					entries.from = entries.from === 0 ? ++entries.from : entries.from;
					entries.to++;
					entries.all++;
					RenderEntriesInfo(entries, label);
					const message = "Employee have been added successfully.";
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification(message);
				},
				400: function (response) {
					const data = JSON.parse(response.responseText);
					$("#manageItems").data("validator").showErrors(data);
				},
				500: function () {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("Internal server error, please try again.");
				}
			}
		});
		break;
	}
}

$(window).on("load", function () {
  LoadData(query, itemsTable);

  EnableSearch(query, itemsTable);
  EnableSorting(query, itemsTable);
  EnableMobileSorting(query, itemsTable);
  EnablePagination(query, itemsTable);

  $("#new").on("click", function () {
	$('#manageModal button[type="submit"]').text("Add");
	$("#manageModalLabel").text("New Employee");
	ClearInputs();
  });

  $("#manageModal").on("shown.bs.modal", function () {
	$(this).find("input:first").focus();
	$("#addBtn").attr("disabled", "disabled");
	origForm = getFormDataAsJson($(itemForm));
  });

  $("#manageModal, #deleteModal").on("hidden.bs.modal", function () {
	ClearInputs();
  });

  EnableAccordionInMobiles(itemsTable);

  ConfigureEdit();
  ConfigureDelete(itemsTable, itemToDeleteId);
  ConfigureSubmit(itemForm.id, itemsTable);

  $(itemForm).data("validator").settings.ignore = null;

  $(itemForm).on("change input blur keyup paste", "input", function () {
	  if (getFormDataAsJson($(itemForm)) !== origForm && $(itemForm).valid())
		  $("#addBtn").removeAttr("disabled");
	  else $("#addBtn").attr("disabled", "disabled");
  });

  var departmenttypeaheadResources = new window.Bloodhound({
		datumTokenizer: window.Bloodhound.tokenizers.obj.whitespace("searchQuery"),
		queryTokenizer: window.Bloodhound.tokenizers.whitespace,
		remote: {
			url: "/api/departments/?withTotal=false&page.number=1&page.size=20&sort.by=name&searchQuery=%searchQuery",
			wildcard: "%searchQuery"
		}
	});

  $("#departmentName").typeahead({
		minLength: 2,
		highlight: true
	}, {
		name: "department",
		display: "name",
		limit: 20,
		source: departmenttypeaheadResources
	}).on("typeahead:select", function (e, department) {
		selectedDepartment = department;
		$("#DepartmentId").val(department.id);
	});

	$("#departmentName").on("blur keyup paste keydown", function() {
		if (selectedDepartment !== null && $(this).val().trim() !== selectedDepartment.name)
			$("#DepartmentId").val("");
	});

	$("#departmentName").on("keypress", function(e) {
		if ($(this).val().length < 2 && e.keyCode === 32)
			e.preventDefault();
	});
});

$(function () {
	jQuery.validator.addMethod("dateOfBirthRange", function (value) {
		if (!value) return false;
		
		const valueYear = new Date(value).getYear();
		const thisYear = new Date().getYear();

		const difference = thisYear - valueYear;
		return difference >= 18 && difference <= 60;
	});

	jQuery.validator.unobtrusive.adapters.add("dateOfBirthRange", [], function (options) {
		const params = { };
		options.rules["dateOfBirthRange"] = params;
		if (options.message) {
			options.messages["dateOfBirthRange"] = options.message;
		}
	});
}(jQuery));