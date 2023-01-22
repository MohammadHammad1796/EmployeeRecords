const apiUrl = "/api/departments/";
const label = "departments";
const itemsTable = $("#items");
const itemForm = $("#manageItems")[0];
let origForm = null;
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

function AddRow(table, department, isOdd) {
  if (isOdd == null)
	isOdd = $(table).find("tr:last").hasClass("odd") ? true : false; 
  let row = `<tr id='item_${department.id}'`;
  if (!isOdd) row += 'class="odd"';
  row += `><td head='Name'>${department.name}</td>
						<td head='Actions'>
						<a href="#" class="edit-btn" data-id="${department.id}"><i class="fas fa-edit me-3 mb-3"></i></a>
						<a href="#" class="remove-btn" data-id="${department.id}"><i class="fas fa-trash"></i></a>
						</td>
						</tr>`;
  $(table).find("tbody:eq(0)").append(row);
}

function ClearInputs() {
  $(itemForm)[0].reset();
  $("#Id").val(0);
  
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
	const  name = tableRow.children(":eq(0)").text();
	$("#Name").val(name);

	$('#manageModal button[type="submit"]').text("Save");
	$("#manageModalLabel").text(`Edit department: ${name}`);
	$("#manageModal").modal("toggle");
  });
}

function ConfigureDelete(table, id) {
  $(table).on("click", "a.remove-btn", function (e) {
	e.preventDefault();
	id = $(this).attr("data-id");const tableRow = $(`#item_${id}`);
	const name = tableRow.children(":eq(0)").text();

	$("#deleteParagraph").html(`<p>Are you sure you want to delete this department?</p>
		<div class="col-md-6 col-8">
			<div>Name: ${name}</div>
		</div>`);
	$("#deleteModal").modal("toggle");
  });

  $("#deleteItem").submit(function (e) {
	e.preventDefault();
	DeleteItem(id, "Department");
  });
}

function UpdateRow(id, departmentName) {
  const row = $(`${id}`);
  row.find("td").eq(0).text(departmentName);
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
					window.DisplayToastNotification("Department not found.");
				},
				204: function() {

					UpdateRow(`#item_${id}`, $("#Name").val());
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
				201: function (response) {
					const department = { name: $("#Name").val(), id: response.id };
					console.log(department);
					AddRow(table, department, null);
					entries.from = entries.from === 0 ? ++entries.from : entries.from;
					entries.to++;
					entries.all++;
					RenderEntriesInfo(entries, label);
					const message = "Department have been added successfully.";
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

$(window).on("load", function() {
	LoadData(query, itemsTable);

	EnableSearch(query, itemsTable);
	EnableSorting(query, itemsTable);
	EnableMobileSorting(query, itemsTable);
	EnablePagination(query, itemsTable);

	$("#new").on("click",
		function() {
			$('#manageModal button[type="submit"]').text("Add");
			$("#manageModalLabel").text("New Department");
			ClearInputs();
		});

	$("#manageModal").on("shown.bs.modal",
		function() {
			$(this).find("input:first").focus();
			$("#addBtn").attr("disabled", "disabled");
			origForm = getFormDataAsJson($(itemForm));
		});

	$("#manageModal, #deleteModal").on("hidden.bs.modal",
		function() {
			ClearInputs();
		});

	EnableAccordionInMobiles(itemsTable);

	ConfigureEdit();
	ConfigureDelete(itemsTable, itemToDeleteId);
	ConfigureSubmit(itemForm.id, itemsTable);

	$(itemForm).data("validator").settings.ignore = null;

	$(itemForm).on("change input blur keyup paste",
		"input",
		function() {
			if (getFormDataAsJson($(itemForm)) !== origForm && $(itemForm).valid())
				$("#addBtn").removeAttr("disabled");
			else $("#addBtn").attr("disabled", "disabled");
		});
});