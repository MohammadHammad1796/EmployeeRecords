const apiUrl = `/api/employees/${employeeId}/files/`;
const label = "files";
var employeeName;
const itemsTable = $("#items");
const itemForm = $("#manageItems")[0];
let origForm = null;
var $ = window.$;

var query = {
	page: {
		number: 1,
		size: 5
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

function AddRow(table, file, isOdd) {
  if (isOdd == null)
	isOdd = $(table).find("tr:last").hasClass("odd") ? true : false; 
  let row = `<tr id='item_${file.id}'`;
  if (!isOdd) row += 'class="odd"';
  row += `><td head='Name'>${file.name}</td>
						<td head='Size' >${file.size} Mb</td>`;
  const fileExtension = file.path.split(".").pop().toLowerCase();
  switch(fileExtension) {
	case "pdf":
		row += `<td head='Path'><a><i class="fas fa-file-pdf"></i></a></td>`;
		break;
	case "dot":
	case "doc":
	case "docx":
		row += `<td head='Path'><a><i class="fas fa-file-word"></i></a></td>`;
		break;
	default:
		row += `<td head='Path'><img src="/${file.path}"/></td>`;
  }
  
	row += `<td head='Actions'>
						<a download="${employeeName} - ${file.name}.${fileExtension}" href="/${file.path}"><i class="fas fa-save me-3 mb-3"></i></a>
						<a href="#" class="edit-btn" data-id="${file.id}"><i class="fas fa-edit me-3 mb-3"></i></a>
						<a href="#" class="remove-btn" data-id="${file.id}"><i class="fas fa-trash"></i></a>
						</td>
						</tr>`;
  $(table).find("tbody:eq(0)").append(row);
}

function ClearInputs() {
  $(itemForm)[0].reset();
  $("#fileId").val(0);
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

	const saveResource = getFormData($(this));
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
	$("#fileId").val(id);
	const  fileName = tableRow.children(":eq(0)").text();
	$("#Name").val(fileName);

	$('#manageModal button[type="submit"]').text("Save");
	$("#manageModalLabel").text(`Edit file: ${fileName}`);
	$("#manageModal").modal("toggle");
  });
}

function ConfigureDelete(table, id) {
  $(table).on("click", "a.remove-btn", function (e) {
	e.preventDefault();
	id = $(this).attr("data-id");

	$("#deleteParagraph").html("<p>Are you sure you want to delete this file?</p>");
	$("#deleteModal").modal("toggle");
  });

  $("#deleteItem").submit(function (e) {
	e.preventDefault();
	DeleteItem(id, "File");
  });
}

function UpdateRow(id, file) {
  const row = $(`${id}`);
  row.find("td").eq(0).text(file.name);
  row.find("td").eq(1).text(`${file.size} Mb`);
  var html;
  const fileExtension = file.path.split(".").pop().toLowerCase();
  switch(fileExtension) {
  case "pdf":
	  html = `<a><i class="fas fa-file-pdf"></i></a>`;
	  break;
  case "dot":
  case "doc":
  case "docx":
	  html = `<a><i class="fas fa-file-word"></i></a>`;
	  break;
  default:
	  html = `<img src="/${file.path}"/>`;
  }
  row.find("td").eq(2).html(html);
  row.find("td").eq(3).find("a:eq(0)").attr("href", `/${file.path}`);
  row.find("td").eq(3).find("a:eq(0)").attr("download", `${employeeName} - ${file.name}.${fileExtension}`);
}

function ConvertSaveResourceToDisplayResource(saveResource, id, size, path) {
	const resource = {
		id: id, 
		name: saveResource.get("Name"), 
		size: size,
		path: path
	};
	return resource;
}

function SaveItem(saveResource, saveType, table) {
	console.log(saveResource);
	switch (saveType) {
	case "Save":
		var id = $("#fileId").val();
		$.ajax({
			async: false,
			url: apiUrl + id,
			method: "PUT",
			data: saveResource,
			processData: false,
			contentType: false,
			statusCode: {
				400: function(response) {
					const data = JSON.parse(response.responseText);
					$("#manageItems").data("validator").showErrors(data);
				},
				404: function() {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("File not found.");
				},
				204: function() {
					$(`#item_${id}`).find("td").eq(0).text(saveResource.get("Name"));
					const message = "Changes have been saved successfully.";
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification(message);
				},
				200: function(response) {
					const fileSize = ConvertFileSizeFromByteToMegaByte(saveResource.get("File").size).toFixed(2);
					const resource = ConvertSaveResourceToDisplayResource(
						saveResource, id, fileSize, response.path);
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
			data: saveResource,
			processData: false,
			contentType: false,
			statusCode: {
				201: function (response) {
					const fileSize = ConvertFileSizeFromByteToMegaByte(saveResource.get("File").size).toFixed(2);
					const resource = ConvertSaveResourceToDisplayResource(
						saveResource, response.id, fileSize, response.path);
					AddRow(table, resource, null);
					entries.from = entries.from === 0 ? ++entries.from : entries.from;
					entries.to++;
					entries.all++;
					RenderEntriesInfo(entries, label);
					const message = "File have been added successfully.";
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

function GetAndRenderEmployeeInfo() {
	const employee = GetData(`/api/employees/${employeeId}`, {});
	employeeName = employee.name;
	const html = `<div class="row">
<div class="col-md-3 col-sm-6 col-12"><p>Name: ${employee.name}</p></div>
<div class="col-md-3 col-sm-6 col-12"><p>Department: ${employee.department.name}</p></div>
<div class="col-md-3 col-sm-6 col-12"><p>Date of birth: ${employee.dateOfBirth}</p></div>
<div class="col-md-3 col-sm-6 col-12"><p>Address: ${employee.address}</p></div>
</div>`;
	$("#info").html(html);
}

$(window).on("load", function () {

	GetAndRenderEmployeeInfo();
  LoadData(query, itemsTable);

  EnableSearch(query, itemsTable);
  EnableSorting(query, itemsTable);
  EnableMobileSorting(query, itemsTable);
  EnablePagination(query, itemsTable);

  $("#new").on("click", function () {
	$('#manageModal button[type="submit"]').text("Add");
	$("#fileId").val(0);
	$("#manageModalLabel").text("New File");
	ClearInputs();
  });

  $("#manageModal").on("shown.bs.modal", function () {
	$(this).find("input:first").focus();
	$("#addBtn").attr("disabled", "disabled");
	origForm = getFormData($(itemForm));
  });

  $("#manageModal, #deleteModal").on("hidden.bs.modal", function () {
	ClearInputs();
  });

  EnableAccordionInMobiles(itemsTable);

  ConfigureEdit();
  ConfigureDelete(itemsTable, itemToDeleteId);
  ConfigureSubmit(itemForm.id, itemsTable);

  $(itemForm).data("validator").settings.ignore = null;

  $(itemForm).on("change input blur keyup paste", "input, textarea", function () {
	  const newFormData = getFormData($(itemForm));
	
	  let formDataEquals = true;
	  for (let [key, value] of newFormData.entries())
		  if (origForm.get(key) !== value) {
			  formDataEquals = false;
			  break;
		  }

	  if (!formDataEquals && $(itemForm).valid()) $("#addBtn").removeAttr("disabled");
	  else $("#addBtn").attr("disabled", "disabled");
  });
});

$(function () {
	jQuery.validator.addMethod("requiredIfIdZero", function (value) {
		var isValid = true;
		if (parseInt($("#fileId").val()) === 0 && (value === null || value === undefined || value === ""))
			isValid = false;
		return isValid;
	});

	jQuery.validator.unobtrusive.adapters.add("requiredIfIdZero", [], function (options) {
		options.rules["requiredIfIdZero"] = {};
		options.messages["requiredIfIdZero"] = options.message;
	});
}(jQuery));

$(function () {
	jQuery.validator.addMethod("maximumFileSizeIfHaveVale", function (value) {
		if (value === null || value === undefined || value === "")
			return true;

		const fileSizeInB = $("#File")[0].files[0].size;
		const sizeInMb = ConvertFileSizeFromByteToMegaByte(fileSizeInB);
		var isValid = true;
		if (sizeInMb > 5)
			isValid = false;

		return isValid;
	});

	jQuery.validator.unobtrusive.adapters.add("maximumFileSizeIfHaveVale", [], function (options) {
		options.rules["maximumFileSizeIfHaveVale"] = {};
		options.messages["maximumFileSizeIfHaveVale"] = options.message;
	});
}(jQuery));