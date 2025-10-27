/* MINIGAME AREA 統一 JavaScript 功能 */

$(document).ready(function() {
    // 初始化所有頁面功能
    initializeCommonFeatures();
    
    // 初始化表格功能
    initializeTables();
    
    // 初始化模態框功能
    initializeModals();
    
    // 初始化搜尋功能
    initializeSearch();
    
    // 初始化載入動畫
    initializeLoading();
});

// 初始化通用功能
function initializeCommonFeatures() {
    // 統一按鈕懸停效果
    $('.btn').hover(
        function() {
            $(this).addClass('btn-hover');
        },
        function() {
            $(this).removeClass('btn-hover');
        }
    );

    // 統一卡片懸停效果
    $('.card').hover(
        function() {
            $(this).addClass('card-hover');
        },
        function() {
            $(this).removeClass('card-hover');
        }
    );

    // 統一表格行懸停效果
    $('.table tbody tr').hover(
        function() {
            $(this).addClass('table-row-hover');
        },
        function() {
            $(this).removeClass('table-row-hover');
        }
    );
}

// 初始化表格功能
function initializeTables() {
    // 為所有表格添加統一樣式
    $('.table').each(function() {
        $(this).addClass('table-hover');
    });
    
    // 初始化 DataTable（如果存在）
    if ($.fn.DataTable) {
        $('.data-table').DataTable({
            "language": {
                "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Chinese-traditional.json"
            },
            "pageLength": 10,
            "order": [[0, "desc"]],
            "responsive": true,
            "dom": '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                   '<"row"<"col-sm-12"tr>>' +
                   '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            "columnDefs": [
                { "orderable": false, "targets": -1 }
            ]
        });
    }
}

// 初始化模態框功能
function initializeModals() {
    // 統一模態框動畫
    $('.modal').on('show.bs.modal', function() {
        $(this).addClass('fade-in');
    });

    $('.modal').on('hidden.bs.modal', function() {
        $(this).removeClass('fade-in');
    });

    // 統一表單驗證
    $('.modal form').on('submit', function(e) {
        e.preventDefault();
        validateForm($(this));
    });
}

// 初始化搜尋功能
function initializeSearch() {
    // 統一搜尋框功能
    $('.search-input').on('keypress', function(e) {
        if (e.which === 13) {
            performSearch($(this));
        }
    });

    $('.search-btn').on('click', function() {
        var searchInput = $(this).closest('.input-group').find('.search-input');
        performSearch(searchInput);
    });
}

// 初始化載入動畫
function initializeLoading() {
    // 統一載入動畫顯示
    window.showLoading = function(element) {
        var loadingHtml = '<div class="loading"></div>';
        if (element) {
            $(element).html(loadingHtml);
        } else {
            $('body').append('<div class="loading-overlay">' + loadingHtml + '</div>');
        }
    };

    // 統一載入動畫隱藏
    window.hideLoading = function(element) {
        if (element) {
            $(element).find('.loading').remove();
        } else {
            $('.loading-overlay').remove();
        }
    };
}

// 執行搜尋
function performSearch(searchInput) {
    var searchTerm = searchInput.val().trim();
    if (searchTerm) {
        // 顯示載入動畫
        showLoading();

        // 執行搜尋邏輯（由各頁面實作）
        if (typeof window.searchFunction === 'function') {
            window.searchFunction(searchTerm);
        }

        // 隱藏載入動畫
        setTimeout(function() {
            hideLoading();
        }, 500);
    }
}

// 表單驗證
function validateForm(form) {
    var isValid = true;
    var errorMessages = [];

    // 檢查必填欄位
    form.find('[required]').each(function() {
        var field = $(this);
        var value = field.val().trim();

        if (!value) {
            isValid = false;
            field.addClass('is-invalid');
            errorMessages.push(field.attr('name') + ' 為必填欄位');
        } else {
            field.removeClass('is-invalid');
        }
    });

    // 檢查電子郵件格式
    form.find('input[type="email"]').each(function() {
        var field = $(this);
        var value = field.val().trim();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (value && !emailRegex.test(value)) {
            isValid = false;
            field.addClass('is-invalid');
            errorMessages.push('請輸入有效的電子郵件格式');
        }
    });

    // 檢查數字格式
    form.find('input[type="number"]').each(function() {
        var field = $(this);
        var value = field.val().trim();
        var min = field.attr('min');
        var max = field.attr('max');

        if (value) {
            var numValue = parseFloat(value);
            if (min && numValue < parseFloat(min)) {
                isValid = false;
                field.addClass('is-invalid');
                errorMessages.push(field.attr('name') + ' 不能小於 ' + min);
            }
            if (max && numValue > parseFloat(max)) {
                isValid = false;
                field.addClass('is-invalid');
                errorMessages.push(field.attr('name') + ' 不能大於 ' + max);
            }
        }
    });

    if (!isValid) {
        showAlert('請修正以下錯誤：', errorMessages.join('<br>'), 'danger');
    }

    return isValid;
}

// 顯示警告訊息
function showAlert(title, message, type) {
    var alertHtml = '<div class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
        '<strong>' + title + '</strong><br>' +
        message +
        '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
        '</div>';

    // 移除現有的警告訊息
    $('.alert').remove();

    // 顯示新的警告訊息
    $('body').prepend(alertHtml);

    // 自動隱藏警告訊息
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);
}

// 格式化日期
function formatDate(dateString) {
    var date = new Date(dateString);
    return date.toLocaleDateString('zh-TW', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// 格式化數字
function formatNumber(number) {
    return new Intl.NumberFormat('zh-TW').format(number);
}

// 確認對話框
function confirmAction(message, callback) {
    if (confirm(message)) {
        if (typeof callback === 'function') {
            callback();
        }
    }
}

// 統一 AJAX 錯誤處理
$(document).ajaxError(function(event, xhr, settings, thrownError) {
    var errorMessage = '操作失敗';

    if (xhr.status === 404) {
        errorMessage = '找不到請求的資源';
    } else if (xhr.status === 500) {
        errorMessage = '伺服器內部錯誤';
    } else if (xhr.status === 403) {
        errorMessage = '沒有權限執行此操作';
    } else if (xhr.status === 401) {
        errorMessage = '請先登入';
    }

    showAlert('錯誤', errorMessage, 'danger');
});

// 統一成功訊息處理
function showSuccess(message) {
    showAlert('成功', message, 'success');
}

// 統一錯誤訊息處理
function showError(message) {
    showAlert('錯誤', message, 'danger');
}

// 統一警告訊息處理
function showWarning(message) {
    showAlert('警告', message, 'warning');
}

// 統一資訊訊息處理
function showInfo(message) {
    showAlert('資訊', message, 'info');
}

// 載入狀態管理
function showLoading(element) {
    if (element) {
        $(element).find('.skeleton-loader').show();
    } else {
        $('#loadingSpinner').show();
    }
}

function hideLoading(element) {
    if (element) {
        $(element).find('.skeleton-loader').hide();
    } else {
        $('#loadingSpinner').hide();
    }
}

// 錯誤處理增強
function showError(message, retryCallback) {
    var toastHtml = `
        <div class="toast align-items-center text-white bg-danger border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    $("#toastContainer").html(toastHtml);
    $('.toast').toast('show');
    
    if (retryCallback) {
        window.retryLoad = retryCallback;
        $('#errorRetry').show();
    }
}

function showSuccess(message) {
    var toastHtml = `
        <div class="toast align-items-center text-white bg-success border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    $("#toastContainer").html(toastHtml);
    $('.toast').toast('show');
}

// AJAX 請求增強
function makeAjaxRequest(url, data, successCallback, errorCallback) {
    showLoading();
    
    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function(response) {
            hideLoading();
            if (response.success) {
                successCallback(response);
            } else {
                showError(response.message || '操作失敗');
                if (errorCallback) errorCallback(response);
            }
        },
        error: function(xhr, status, error) {
            hideLoading();
            showError('網路錯誤：' + error, function() {
                makeAjaxRequest(url, data, successCallback, errorCallback);
            });
            if (errorCallback) errorCallback(xhr, status, error);
        }
    });
}

// 表單驗證增強
function validateForm(form) {
    var isValid = true;
    var errors = [];
    
    // 檢查必填欄位
    form.find('[required]').each(function() {
        if (!$(this).val().trim()) {
            isValid = false;
            errors.push($(this).attr('name') + ' 為必填欄位');
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    
    // 檢查電子郵件格式
    form.find('input[type="email"]').each(function() {
        var email = $(this).val();
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (email && !emailRegex.test(email)) {
            isValid = false;
            errors.push('電子郵件格式不正確');
            $(this).addClass('is-invalid');
        }
    });
    
    // 檢查數字格式
    form.find('input[type="number"]').each(function() {
        var value = $(this).val();
        var min = $(this).attr('min');
        var max = $(this).attr('max');
        
        if (value && (min && parseFloat(value) < parseFloat(min))) {
            isValid = false;
            errors.push($(this).attr('name') + ' 不能小於 ' + min);
            $(this).addClass('is-invalid');
        }
        
        if (value && (max && parseFloat(value) > parseFloat(max))) {
            isValid = false;
            errors.push($(this).attr('name') + ' 不能大於 ' + max);
            $(this).addClass('is-invalid');
        }
    });
    
    if (!isValid) {
        showError('表單驗證失敗：' + errors.join(', '));
    }
    
    return isValid;
}

// 搜尋功能增強
function initializeSearch() {
    // 即時搜尋
    $('.search-input').on('input', function() {
        var searchTerm = $(this).val();
        var table = $(this).closest('.card').find('.table');
        
        if (table.length) {
            table.DataTable().search(searchTerm).draw();
        }
    });
    
    // 搜尋建議
    $('.search-input').on('focus', function() {
        $(this).closest('.search-container').addClass('search-focused');
    });
    
    $('.search-input').on('blur', function() {
        $(this).closest('.search-container').removeClass('search-focused');
    });
}

// 鍵盤快捷鍵
function initializeKeyboardShortcuts() {
    $(document).on('keydown', function(e) {
        // Ctrl + F 聚焦搜尋框
        if (e.ctrlKey && e.key === 'f') {
            e.preventDefault();
            $('.search-input').focus();
        }
        
        // Ctrl + R 重新載入
        if (e.ctrlKey && e.key === 'r') {
            e.preventDefault();
            location.reload();
        }
        
        // Esc 關閉模態框
        if (e.key === 'Escape') {
            $('.modal').modal('hide');
        }
    });
}

// 離線狀態檢測
function initializeOfflineDetection() {
    window.addEventListener('online', function() {
        showSuccess('網路連接已恢復');
        $('#offlineIndicator').hide();
    });
    
    window.addEventListener('offline', function() {
        showError('網路連接已斷開，部分功能可能無法使用');
        $('#offlineIndicator').show();
    });
}

// 初始化所有功能
$(document).ready(function() {
    initializeCommonFeatures();
    initializeTables();
    initializeModals();
    initializeSearch();
    initializeLoading();
    initializeKeyboardShortcuts();
    initializeOfflineDetection();
});
