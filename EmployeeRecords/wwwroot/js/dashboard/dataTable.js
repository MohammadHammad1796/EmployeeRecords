function RenderPaginationButtons(pageNumber, pageSize, count) {
    const availablePages =
        count % pageSize === 0
            ? Math.floor(count / pageSize)
            : Math.floor(count / pageSize) + 1;
    let minPage, maxPage;
    if (pageNumber <= 3) minPage = 1;
    else minPage = pageNumber - 2;
    maxPage = minPage + 4;
    if (maxPage > availablePages) maxPage = availablePages;
    if (maxPage - minPage < 4) {
        if (maxPage - minPage < availablePages) {
            minPage = 1;
        }
    }
    $("#paginationSection input").remove();
    for (let current = maxPage, i = 0; current >= minPage && i < 5; current--, i++) {
        let button = `<input type='button' value='${current}' class='btn me-1 ms-1 `;
        if (current === parseInt(pageNumber)) button += "active";
        button += "' ";
        if (current === parseInt(pageNumber)) button += "disabled='disabled'";
        button += " />";
        $("#paginationSection a:eq(0)").after(button);
    }
    if (pageNumber - 1 >= minPage) {
        $("#paginationSection a:eq(0)").val(pageNumber - 1);
        $("#paginationSection a:eq(0)").attr("href", "#");
    } else {
        $("#paginationSection a:eq(0)").val(0);
        $("#paginationSection a:eq(0)").removeAttr("href");
    }
    if (parseInt(pageNumber) + 1 <= maxPage) {
        $("#paginationSection a:eq(1)").val(parseInt(pageNumber) + 1);
        $("#paginationSection a:eq(1)").attr("href", "#");
    } else {
        $("#paginationSection a:eq(1)").val(0);
        $("#paginationSection a:eq(1)").removeAttr("href");
    }
}

function CalculateEntries(
    count,
    queryObject,
    currentItemsCount,
    entriesObject
) {
    entriesObject.from = queryObject.page.size * (queryObject.page.number - 1);
    if (currentItemsCount)
        entriesObject.from++;
    entriesObject.to =
        currentItemsCount < queryObject.page.size
        ? queryObject.page.size * (queryObject.page.number - 1) + currentItemsCount
        : queryObject.page.size * queryObject.page.number;
    entriesObject.all = count;
}

function RenderEntriesInfo(entriesObject, label) {
    $("#countInfo").text(
        `Showing ${entriesObject.from} to ${entriesObject.to} of ${entriesObject.all} ${label}`
    );
}

function convertQueryObjectToQueryString(queryObject, name = "") {
    let queryString = "";
    for (let key in queryObject) {
        if (queryObject.hasOwnProperty(key)) {
            const value = queryObject[key];
            if (typeof value === "object") {
                queryString += convertQueryObjectToQueryString(value, key);
            } else {
                if (name) queryString += `${name}.`;
                queryString += `${key}=${value}&`;
            }
        }
    }
    return queryString;
}

function GetData(url, queryObject) {
    let result = null;
    const queryString = convertQueryObjectToQueryString(queryObject);
    $.ajax({
        url: `${url}?${queryString}`,
        type: "GET",
        async: false,
        statusCode: {
            200: function (response) {
                result = response;
            },
            404: function() { },
            500: function() {
                if (queryObject && "withTotal" in queryObject)
                    result = queryObject.withTotal ? { total: 0, items: [] } : { items: [] };
            }
        }
    });
    return result;
}

function RenderItemsTable(items, table) {
    const tbody = $(table).find("tbody:eq(0)");
    tbody.empty();
    let isOdd = false;
    for (let item of items) {
        if (!isOdd) isOdd = true;
        else isOdd = false;
        AddRow(table, item, isOdd);
    }
}

function LoadData(queryObject, table) {
    var resource = GetData(apiUrl, queryObject);
    if (!resource.items.length && parseInt(queryObject.page.number) !== 1) {
        queryObject.page.number = 1;
        resource = GetData(apiUrl, queryObject);
    }
    RenderItemsTable(resource.items, table);
  
    RenderPaginationButtons(
        queryObject.page.number,
        queryObject.page.size,
        resource.total
    );
    CalculateEntries(resource.total, query, resource.items.length, entries);
    RenderEntriesInfo(entries, label);
    resource = null;
}

function getFormDataAsJson(form) {
    const unIndexedArray = form.serializeArray();
    const indexedArray = {};

    $.map(unIndexedArray, function (n) {
        if (typeof n["value"] === 'string')
            n["value"] = n["value"].trim();
        indexedArray[n["name"]] = n["value"];
    });

    const result = JSON.stringify(indexedArray);
    return result;
}

function getFormData(form) {
    const unIndexedArray = form.serializeArray();
    var data = new FormData();
    $.map(unIndexedArray, function (n) {
        if (typeof n["value"] === "string")
            n["value"] = n["value"].trim();
        data.append(n["name"], n["value"]);
    });

    $(form).find("input[type='file']").each(function () {
        if ($(this).val() !== "")
            data.append($(this).attr("id"), $(this).get(0).files[0]);
    });

    return data;
}

function EnableSearch(queryObject, table) {
    $("#searchText").on("change paste keyup", function () {
        let searchText = $(this).val().trim();
        searchText = searchText.replace("[ ]{2,}", " ");
        if (searchText.length < 1)
            if (queryObject.searchQuery === "") return;
            else {
                queryObject.searchQuery = "";
                LoadData(queryObject, table);
                return;
            }

        query.searchQuery = searchText;
        LoadData(queryObject, table);
    });
}

function EnableSorting(queryObject, table) {
    $(".sort").on("click", function () {
        $(this).siblings().find('i').removeClass('fa-sort-up fa-sort-down');
        const child = $(this).children("i:eq(0)");
        queryObject.sort.by = $(this).attr('sortby');
        $(`#sortBy option[value='${$(this).attr('sortby')}']`).prop("selected", true);
        if (!child.hasClass("fa-sort-up") && !child.hasClass("fa-sort-down")) {
            child.addClass("fa-sort-down");
            $("[name='sort'][value='desc']").prop("checked", true);
            queryObject.sort.isAscending = false;
            LoadData(queryObject, table);
            return;
        }

        if (child.hasClass("fa-sort-up"))  {
            queryObject.sort.isAscending = false;
            $("[name='sort'][value='desc']").prop("checked", true);
        }
        else {
            queryObject.sort.isAscending = true;
            $("[name='sort'][value='asc']").prop("checked", true);
        }

        child.toggleClass("fa-sort-up fa-sort-down");
        LoadData(queryObject, table);
    });
}

function EnablePagination(queryObject, table) {
    $("#pageSize").on("change", function () {
        queryObject.page.size = $(this).val();
        LoadData(queryObject, table);
    });

    $("#paginationSection").on("click", "input[type=button]", function (e) {
        e.preventDefault();
        if ($(this).hasClass("active")) return;

        queryObject.page.number = $(this).val();
        LoadData(queryObject, table);
    });

    $("#paginationSection").on("click", "a", function (e) {
        e.preventDefault();
        if (!$(this).attr("href")) return;

        queryObject.page.number = $(this).val();
        LoadData(queryObject, table);
    });
}

function EnableMobileSorting(queryObject, table) {
    $("[name='sort']").on("click", function () {

        const value = $(this).val();
        if (value === "asc" && queryObject.sort.isAscending === true || 
            value === "desc" && queryObject.sort.isAscending === false)
            return;

        if (value === "asc") {
            $('.sort').find('i.fa-sort-down').addClass('fa-sort-up').removeClass('fa-sort-down');
            queryObject.sort.isAscending = true;
            LoadData(queryObject, table);
            return;
        }
        $('.sort').find('i.fa-sort-up').addClass('fa-sort-down').removeClass('fa-sort-up');
        queryObject.sort.isAscending = false;
        LoadData(queryObject, table);
    });

    $("#sortBy").on("click", function() {
        const value = $(this).val();
        if (value === queryObject.sort.by)
            return;

        $(".sort i").removeClass("fa-sort-up fa-sort-down");
        if (queryObject.sort.isAscending)
            $(`.sort[sortby='${value}'] i`).eq(0).addClass("fa-sort-up");
        else
            $(`.sort[sortby='${value}'] i`).eq(0).addClass("fa-sort-down");
        queryObject.sort.by = value;
        LoadData(queryObject, table);
    });
}

function DeleteItem(id, itemType) {
    $.ajax({
        async: false,
        url: `${apiUrl}${id}`,
        method: "DELETE",
        statusCode: {
            404: function () {
                $("#deleteModal").modal("toggle");
                window.DisplayToastNotification(`${itemType} not found.`);
            },
            204: function () {
                $(`#item_${id}`).remove();
                entries.from = entries.to === 1 ? --entries.from : entries.from;
                entries.to--;
                entries.all--;
                if (entries.from > entries.to) {
                    query.page.number--;
                    LoadData(query, itemsTable);
                }
                else
                    RenderEntriesInfo(entries, label);
                $("#deleteModal").modal("toggle");
                window.DisplayToastNotification(`${itemType} have been deleted successfully.`);
            },
            500: function () {
                $("#deleteModal").modal("toggle");
                window.DisplayToastNotification("Internal server error, please try again.");
            }
        }
    });
}

function EnableAccordionInMobiles(itemsTable) {
    $(itemsTable).on("click", "tr td:first-child", function () {
        if ($(this).css("display") !== "block") return;

        if ($(this).siblings().eq(0).css("display") !== "block") {
            $(this).siblings().show(200,
                function() {
                    $(this).css("display", "block");
                });
            $(this)
                .parent()
                .siblings()
                .find("td:not(:first-child)")
                .css("display", "none");
            return;
        }
        $(this).siblings().hide(200);
    });
}