// ===== GLOBAL VARIABLES =====
let isFiltering = false;
let currentFilter = null;

window.contractWizardData = {
    room: {},
    contract: {},
    tenant: {}
};

// ===== FORM HANDLING =====
function clearAllFormInputs() {
    // Clear contract info form
    const contractInputs = ['ngayBatDau', 'ngayKetThuc', 'tienCoc', 'trangThai', 'ghiChu'];
    contractInputs.forEach(inputId => {
        const element = document.getElementById(inputId);
        if (element) {
            if (element.type === 'select-one') {
                element.selectedIndex = 0;
            } else {
                element.value = '';
            }
        }
    });

    // Clear tenant info form
    const tenantInputs = ['hoTen', 'cccd', 'soDienThoai', 'ngaySinh', 'diaChi'];
    tenantInputs.forEach(inputId => {
        const element = document.getElementById(inputId);
        if (element) {
            element.value = '';
        }
    });

    // Clear room info display
    const roomDisplays = ['roomName', 'roomType', 'roomArea', 'roomPrice'];
    roomDisplays.forEach(displayId => {
        const element = document.getElementById(displayId);
        if (element) {
            element.textContent = '';
        }
    });

    // Clear contract preview
    const previewElement = document.getElementById('contractPreviewContent');
    if (previewElement) {
        previewElement.innerHTML = '';
    }
}

// ===== CONTRACT WIZARD FUNCTIONS =====
function showRoomSelectionModal() {
    clearAllFormInputs();
    window.contractWizardData = { room: {}, contract: {}, tenant: {} };
    openModal('roomSelectionModal');
    loadAvailableRooms();
}

function closeAllContractModals() {
    ['roomSelectionModal', 'contractInfoModal', 'tenantInfoModal', 'contractPreviewModal'].forEach(modalId => {
        closeModal(modalId);
    });

    // Reset dữ liệu wizard
    window.contractWizardData = { room: {}, contract: {}, tenant: {} };
    clearAllFormInputs();

    // Reset nút "Lưu hợp đồng"
    const btnSave = document.getElementById('btnSaveContract');
    if (btnSave) {
        btnSave.disabled = false;
        btnSave.innerHTML = '<i class="fas fa-save mr-2"></i>Lưu hợp đồng';
    }

    // Refresh danh sách hợp đồng (nếu đang ở trang hợp đồng)
    if (typeof refreshContracts === 'function') {
        refreshContracts();
    }
}

function loadAvailableRooms() {
    const roomsLoading = document.getElementById('roomsLoading');
    const roomsList = document.getElementById('roomsList');
    const noRoomsMessage = document.getElementById('noRoomsMessage');

    roomsLoading.classList.remove('hidden');
    roomsList.classList.add('hidden');
    noRoomsMessage.classList.add('hidden');

    $.ajax({
        url: '/QuanLy/Phong/GetAvailableRooms',
        method: 'GET',
        success: function (response) {
            roomsLoading.classList.add('hidden');

            if (response.success && response.data?.length > 0) {
                roomsList.classList.remove('hidden');     // đưa lên trước
                displayRooms(response.data);              // nếu lỗi sẽ thấy khối trắng thay vì spinner
            } else {
                noRoomsMessage.classList.remove('hidden');
            }

        },
        error: function (xhr) {
            roomsLoading.classList.add('hidden');
            noRoomsMessage.classList.remove('hidden');
            showNotification('Lỗi khi tải danh sách phòng: ' + xhr.responseText, 'error');
        }
    });
}

function displayRooms(rooms) {
    const roomsContainer = document.querySelector('#roomsList .grid');
    roomsContainer.innerHTML = '';

    rooms.forEach(room => {
        const roomCard = document.createElement('div');
        roomCard.className = 'bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer';
        roomCard.onclick = () => selectRoom(room);

        roomCard.innerHTML = `
            <div class="flex items-center justify-between mb-3">
                <h4 class="text-lg font-semibold text-gray-800">${room.tenPhong}</h4>
                <span class="status-available px-2 py-1 text-xs font-semibold rounded-full text-white bg-green-500">
                    ${room.trangThai}
                </span>
            </div>
            <div class="space-y-2 text-sm text-gray-600">
                <div class="flex justify-between">
                    <span>Loại phòng:</span>
                    <span class="font-medium">${room.loaiPhong}</span>
                </div>
                <div class="flex justify-between">
                    <span>Diện tích:</span>
                    <span class="font-medium">${room.dienTich} m²</span>
                </div>
                <div class="flex justify-between">
                    <span>Giá thuê:</span>
                    <span class="font-medium text-green-600">${formatCurrency(room.giaPhong)}</span>
                </div>
            </div>
            <div class="mt-4 pt-3 border-t border-gray-100">
                <button class="w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 transition-colors">
                    <i class="fas fa-file-contract mr-2"></i>Chọn phòng này
                </button>
            </div>
        `;

        roomsContainer.appendChild(roomCard);
    });
}

function selectRoom(room) {
    window.contractWizardData.room = {
        maPhong: room.maPhong,
        tenPhong: room.tenPhong,
        loaiPhong: room.loaiPhong,
        dienTich: room.dienTich,
        giaPhong: room.giaPhong,
        trangThai: room.trangThai
    };

    // Update UI
    document.getElementById('roomName').textContent = room.tenPhong;
    document.getElementById('roomType').textContent = room.loaiPhong;
    document.getElementById('roomArea').textContent = room.dienTich + ' m²';
    document.getElementById('roomPrice').textContent = formatCurrency(room.giaPhong);

    // Set default values
    document.getElementById('tienCoc').value = room.giaPhong;
    document.getElementById('ngayBatDau').value = new Date().toISOString().split('T')[0];
    document.getElementById('ngayKetThuc').value = new Date(Date.now() + 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
    document.getElementById('trangThai').value = 'Còn hiệu lực';
    document.getElementById('ghiChu').value = '';

    closeModal('roomSelectionModal');
    openModal('contractInfoModal');
}

// ===== WIZARD NAVIGATION =====
function goBackToRoomSelection() {
    closeModal('contractInfoModal');
    clearAllFormInputs();
    window.contractWizardData = { room: {}, contract: {}, tenant: {} };

    if (typeof refreshContracts === 'function') {
        refreshContracts();
    }
}

function goToTenantInfo() {
    if (!validateContractInfo()) return;
    saveContractInfo();
    closeModal('contractInfoModal');
    openModal('tenantInfoModal');
}

function goBackToContractInfo() {
    closeModal('tenantInfoModal');
    openModal('contractInfoModal');
}

function goToContractPreview() {
    if (!validateTenantInfo()) return;
    saveTenantInfo();
    generateContractPreview();
    closeModal('tenantInfoModal');
    openModal('contractPreviewModal');
}

function goBackToTenantInfo() {
    closeModal('contractPreviewModal');
    openModal('tenantInfoModal');
}

// ===== VALIDATION FUNCTIONS =====
function validateContractInfo() {
    const ngayBatDau = document.getElementById('ngayBatDau')?.value;
    const ngayKetThuc = document.getElementById('ngayKetThuc')?.value;
    const tienCoc = document.getElementById('tienCoc')?.value;

    if (!ngayBatDau || !ngayKetThuc || !tienCoc) {
        showNotification('Vui lòng điền đầy đủ thông tin hợp đồng!', 'error');
        return false;
    }
    if (new Date(ngayKetThuc) <= new Date(ngayBatDau)) {
        showNotification('Ngày kết thúc phải sau ngày bắt đầu!', 'error');
        return false;
    }
    return true;
}

function validateTenantInfo() {
    const hoTen = document.getElementById('hoTen')?.value?.trim();
    const cccd = document.getElementById('cccd')?.value?.trim();
    const soDienThoai = document.getElementById('soDienThoai')?.value?.trim();

    if (!hoTen || !cccd || !soDienThoai) {
        showNotification('Vui lòng nhập Họ tên, CCCD và Số điện thoại!', 'error');
        return false;
    }

    if (!/^\d{12}$/.test(cccd)) {
        showNotification('CCCD phải có 12 chữ số!', 'error');
        return false;
    }

    if (!/^\d{10,11}$/.test(soDienThoai)) {
        showNotification('Số điện thoại không hợp lệ!', 'error');
        return false;
    }

    return true;
}

// ===== DATA SAVING =====
function saveContractInfo() {
    window.contractWizardData.contract = {
        ngayBatDau: document.getElementById('ngayBatDau')?.value || '',
        ngayKetThuc: document.getElementById('ngayKetThuc')?.value || '',
        tienCoc: parseFloat(document.getElementById('tienCoc')?.value || 0),
        trangThai: document.getElementById('trangThai')?.value || 'Còn hiệu lực',
        ghiChu: document.getElementById('ghiChu')?.value || ''
    };
}

function saveTenantInfo() {
    window.contractWizardData.tenant = {
        hoTen: document.getElementById('hoTen')?.value?.trim() || '',
        cccd: document.getElementById('cccd')?.value?.trim() || '',
        soDienThoai: document.getElementById('soDienThoai')?.value?.trim() || '',
        ngaySinh: document.getElementById('ngaySinh')?.value || '',
        diaChi: document.getElementById('diaChi')?.value?.trim() || ''
    };
}

// ===== CONTRACT PREVIEW & SAVE =====
function generateContractPreview() {
    const contractContentEl = document.getElementById('contractPreviewContent');
    if (!contractContentEl) return;

    const c = window.contractWizardData.contract;
    const r = window.contractWizardData.room;
    const t = window.contractWizardData.tenant;

    const html = `
        <div style="font-family:'Times New Roman',serif;line-height:1.6;">
            <div class="text-center mb-8">
                <h1 class="text-2xl font-bold mb-2">HỢP ĐỒNG THUÊ PHÒNG TRỌ</h1>
                <p class="text-sm">Số: HD${new Date().getFullYear()}-${String(Date.now()).slice(-6)}</p>
            </div>

            <div class="mb-6">
                <h3 class="font-bold mb-3">BÊN CHO THUÊ (Bên A):</h3>
                <p><strong>Họ và tên:</strong> Đinh Công Vinh</p>
                <p><strong>CCCD:</strong> 123456789</p>
                <p><strong>Địa chỉ:</strong> Số 3 phố Cầu Giấy, P.Láng Thượng, Q.Đống Đa, Tp.Hà Nội.</p>
                <p><strong>Điện thoại:</strong>123456789</p>
            </div>

            <div class="mb-6">
                <h3 class="font-bold mb-3">BÊN THUÊ (Bên B):</h3>
                <p><strong>Họ và tên:</strong> ${t.hoTen}</p>
                <p><strong>CCCD:</strong> ${t.cccd}</p>
                <p><strong>Ngày sinh:</strong> ${t.ngaySinh ? new Date(t.ngaySinh).toLocaleDateString('vi-VN') : 'Chưa cập nhật'}</p>
                <p><strong>Địa chỉ thường trú:</strong> ${t.diaChi || 'Chưa cập nhật'}</p>
                <p><strong>Điện thoại:</strong> ${t.soDienThoai}</p>
            </div>

            <div class="mb-6">
                <h3 class="font-bold mb-3">THÔNG TIN PHÒNG THUÊ:</h3>
                <p><strong>Tên phòng:</strong> ${r.tenPhong}</p>
                <p><strong>Loại phòng:</strong> ${r.loaiPhong}</p>
                <p><strong>Diện tích:</strong> ${r.dienTich} m²</p>
                <p><strong>Giá thuê:</strong> ${formatCurrency(r.giaPhong)}/tháng</p>
            </div>

            <div class="mb-6">
                <h3 class="font-bold mb-3">ĐIỀU KHOẢN HỢP ĐỒNG:</h3>
                <p><strong>Thời hạn thuê:</strong> Từ ${new Date(c.ngayBatDau).toLocaleDateString('vi-VN')} đến ${new Date(c.ngayKetThuc).toLocaleDateString('vi-VN')}</p>
                <p><strong>Tiền cọc:</strong> ${formatCurrency(c.tienCoc)}</p>
                <p><strong>Trạng thái:</strong> ${c.trangThai}</p>
                ${c.ghiChu ? `<p><strong>Ghi chú:</strong> ${c.ghiChu}</p>` : ''}
            </div>
        </div>
    `;
    contractContentEl.innerHTML = html;
}

function saveContract() {
    const data = window.contractWizardData;

    if (!data.room.maPhong) {
        showNotification('Không tìm thấy thông tin phòng!', 'error');
        return;
    }

    if (!data.tenant.hoTen || !data.tenant.cccd || !data.tenant.soDienThoai) {
        showNotification('Thông tin khách thuê không đầy đủ!', 'error');
        return;
    }

    const payload = {
        MaPhong: parseInt(data.room.maPhong),
        NgayBatDau: data.contract.ngayBatDau,
        NgayKetThuc: data.contract.ngayKetThuc,
        TienCoc: parseFloat(data.contract.tienCoc || 0),
        TrangThai: data.contract.trangThai || 'Còn hiệu lực',
        Tenant: {
            HoTen: data.tenant.hoTen,
            Cccd: data.tenant.cccd,
            SoDienThoai: data.tenant.soDienThoai,
            DiaChi: data.tenant.diaChi || null,
            NgaySinh: data.tenant.ngaySinh || null,
            IsExisting: false
        }
    };

    // Tìm button và lưu trạng thái ban đầu
    const saveBtn = document.querySelector('.modal-box:not(.hidden) button[onclick*="saveContract"]') ||
        document.querySelector('button[onclick="saveContract()"]');

    if (!saveBtn) {
        showNotification('Không tìm thấy nút lưu!', 'error');
        return;
    }

    const originalText = saveBtn.innerHTML;

    // Disable button và hiển thị loading
    saveBtn.disabled = true;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Đang lưu...';

    $.ajax({
        url: '/QuanLy/HopDong/CreateWithTenant',
        type: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        data: JSON.stringify(payload),
        success: function (res) {
            if (res && res.success) {
                showNotification('Tạo hợp đồng thành công!', 'success');
                closeAllContractModals();

                if (typeof refreshContracts === 'function') {
                    refreshContracts();
                }
            } else {
                showNotification(res?.message || 'Không lưu được hợp đồng', 'error');
            }
        },
        error: function (xhr) {
            let errorMsg = 'Có lỗi xảy ra khi lưu hợp đồng';
            try {
                const errorResponse = JSON.parse(xhr.responseText);
                errorMsg = errorResponse.message || errorMsg;
            } catch (e) {
                errorMsg = xhr.responseText || xhr.statusText || errorMsg;
            }
            showNotification('Lỗi: ' + errorMsg, 'error');
        },
        complete: function () {
            // Luôn khôi phục lại button dù thành công hay thất bại
            if (saveBtn) {
                saveBtn.disabled = false;
                saveBtn.innerHTML = originalText;
            }
        }
    });
}

function printToPDF() {
    const w = window.open('', '_blank');
    const html = document.getElementById('contractPreviewContent')?.innerHTML || '';
    w.document.write(`
        <!doctype html><html><head>
            <meta charset="utf-8">
            <title>Hợp đồng thuê phòng</title>
            <style>body{font-family:'Times New Roman',serif;line-height:1.6;margin:20px}</style>
        </head><body>${html}</body></html>
    `);
    w.document.close();
    w.print();
}

// ===== UTILITY FUNCTIONS =====
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount ?? 0);
}

function getStatusBadge(status) {
    const s = (status || '').toLowerCase();
    if (s === 'còn hiệu lực')
        return 'px-3 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800';
    if (s === 'đã kết thúc')
        return 'px-3 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800';
    return 'px-3 py-1 text-xs font-semibold rounded-full bg-gray-100 text-gray-800';
}

// ===== FILTER FUNCTIONS =====
function applyFilter(range, button) {
    // Đặt lại tất cả button về trạng thái bình thường
    document.querySelectorAll('.filter-exp').forEach(btn => {
        btn.classList.remove('bg-blue-600', 'text-white');
        btn.classList.add('bg-white', 'text-blue-600');
    });

    // Highlight button được chọn
    button.classList.remove('bg-white', 'text-blue-600');
    button.classList.add('bg-blue-600', 'text-white');

    isFiltering = true;
    currentFilter = range;

    // Gọi API filter
    fetch(`/QuanLy/HopDong/FilterByExpiration?range=${range}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                if (typeof displayContracts === 'function') {
                    displayContracts(data.data);
                }

                if (data.data.length > 0) {
                    document.getElementById('contractsTable').classList.remove('hidden');
                    document.getElementById('noContractsMessage').classList.add('hidden');
                } else {
                    document.getElementById('contractsTable').classList.add('hidden');
                    document.getElementById('noContractsMessage').classList.remove('hidden');
                }
            } else {
                showNotification(data.message, 'error');
                resetFilter();
            }
        })
        .catch(error => {
            showNotification('Lỗi khi lọc hợp đồng!', 'error');
            resetFilter();
        });
}

function resetFilter() {
    // Đặt lại tất cả button
    document.querySelectorAll('.filter-exp').forEach(btn => {
        btn.classList.remove('bg-blue-600', 'text-white');
        btn.classList.add('bg-white', 'text-blue-600');
    });

    isFiltering = false;
    currentFilter = null;

    // Load lại tất cả hợp đồng
    if (typeof loadContracts === 'function') {
        loadContracts();
    }
}

function refreshContracts() {
    if (isFiltering && currentFilter) {
        // Nếu đang filter thì áp dụng lại filter
        const activeButton = document.querySelector(`.filter-exp[data-range="${currentFilter}"]`);
        if (activeButton) {
            applyFilter(currentFilter, activeButton);
        }
    } else {
        // Không filter thì load bình thường
        if (typeof loadContracts === 'function') {
            loadContracts();
        }
    }
}

// ===== EVENT LISTENERS =====
// Close modals when clicking outside
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-backdrop')) {
        const modals = document.querySelectorAll('[id$="Modal"]');
        const contractModals = ['roomSelectionModal', 'contractInfoModal', 'tenantInfoModal', 'contractPreviewModal'];
        let hasContractModal = false;

        modals.forEach(modal => {
            if (!modal.classList.contains('hidden')) {
                if (contractModals.includes(modal.id)) {
                    hasContractModal = true;
                }
                closeModal(modal.id);
            }
        });

        if (hasContractModal) {
            window.contractWizardData = { room: {}, contract: {}, tenant: {} };
            clearAllFormInputs();

            if (typeof refreshContracts === 'function') {
                refreshContracts();
            }
        }
    }
});

// Xử lý click filter buttons
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.filter-exp').forEach(button => {
        button.addEventListener('click', function () {
            const range = this.getAttribute('data-range');

            if (currentFilter === range) {
                // Nếu đang filter cùng loại thì bỏ filter
                resetFilter();
            } else {
                // Áp dụng filter mới
                applyFilter(range, this);
            }
        });
    });
});


