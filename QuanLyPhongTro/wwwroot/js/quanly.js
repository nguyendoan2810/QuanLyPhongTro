// Global Variables
let currentSection = 'dashboard';

// Navigation Functions
function showSection(sectionName) {
    // Hide all sections
    document.querySelectorAll('.section').forEach(section => {
        section.classList.add('hidden');
    });

    // Show selected section
    document.getElementById(sectionName + '-section').classList.remove('hidden');

    // Update page title
    const titles = {
        'dashboard': 'Dashboard',
        'rooms': 'Quản lý Phòng',
        'tenants': 'Quản lý Khách thuê',
        'contracts': 'Hợp đồng thuê',
        'services': 'Quản lý Dịch vụ',
        'bills': 'Quản lý Hóa đơn',
        'finance': 'Thu Chi',
        'reports': 'Báo cáo & Thống kê',
        'accounts': 'Quản lý Tài khoản'
    };
    document.getElementById('pageTitle').textContent = titles[sectionName];

    // Update active nav item
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('nav-item-active');
    });
    event.target.classList.add('nav-item-active');

    // Close sidebar on mobile after selection
    if (window.innerWidth < 1024) {
        toggleSidebar();
    }

    currentSection = sectionName;
}

// Sidebar Functions
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('overlay');

    if (sidebar.classList.contains('-translate-x-full')) {
        sidebar.classList.remove('-translate-x-full');
        overlay.classList.remove('hidden');
    } else {
        sidebar.classList.add('-translate-x-full');
        overlay.classList.add('hidden');
    }
}

// Modal Functions
function openModal(modalId) {
    document.getElementById(modalId).classList.remove('hidden');
    document.body.style.overflow = 'hidden';
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.add('hidden');
    document.body.style.overflow = 'auto';
}

// Utility Functions
function togglePassword(inputId) {
    const input = document.getElementById(inputId);
    const icon = input.nextElementSibling.querySelector('i');

    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        input.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}

function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg max-w-sm bounce-in ${type === 'success' ? 'bg-green-500 text-white' :
            type === 'error' ? 'bg-red-500 text-white' :
                type === 'warning' ? 'bg-yellow-500 text-white' :
                    'bg-blue-500 text-white'
        }`;

    notification.innerHTML = `
                <div class="flex items-center">
                    <i class="fas ${type === 'success' ? 'fa-check-circle' :
            type === 'error' ? 'fa-exclamation-circle' :
                type === 'warning' ? 'fa-exclamation-triangle' :
                    'fa-info-circle'
        } mr-2"></i>
                    <span>${message}</span>
                    <button onclick="this.parentElement.parentElement.remove()" class="ml-4 text-white hover:text-gray-200">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `;

    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}
let chartInstances = {};

// Chart Functions
function initCharts() {
    // Hàm tiện ích: hủy chart cũ nếu tồn tại
    function destroyIfExists(id) {
        if (chartInstances[id]) {
            chartInstances[id].destroy();
            delete chartInstances[id];
        }
    }

    // ===== 1️⃣ Biểu đồ Doanh thu tổng =====
    const revenueCtx = document.getElementById('revenueChart');
    if (revenueCtx && typeof revenueLabels !== 'undefined' && typeof revenueData !== 'undefined') {
        destroyIfExists('revenueChart');
        chartInstances['revenueChart'] = new Chart(revenueCtx, {
            type: 'line',
            data: {
                labels: revenueLabels,
                datasets: [{
                    label: 'Doanh thu (triệu VNĐ)',
                    data: revenueData,
                    borderColor: '#667eea',
                    backgroundColor: 'rgba(102, 126, 234, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#667eea',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: '#f3f4f6' },
                        ticks: { color: '#6b7280' }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { color: '#6b7280' }
                    }
                }
            }
        });
    }

    // ===== 2️⃣ Biểu đồ Tình trạng phòng =====
    const roomCtx = document.getElementById('roomChart');
    if (roomCtx && typeof roomLabels !== 'undefined' && typeof roomData !== 'undefined' && typeof roomColors !== 'undefined') {
        destroyIfExists('roomChart');
        chartInstances['roomChart'] = new Chart(roomCtx, {
            type: 'doughnut',
            data: {
                labels: roomLabels,
                datasets: [{
                    data: roomData,
                    backgroundColor: roomColors,
                    borderWidth: 0,
                    cutout: '70%'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true,
                            color: '#6b7280'
                        }
                    }
                }
            }
        });
    }

    // ===== 3️⃣ Biểu đồ Doanh thu theo tháng =====
    const monthlyRevenueCtx = document.getElementById('monthlyRevenueChart');
    if (monthlyRevenueCtx && typeof monthlyRevenueLabels !== 'undefined' && typeof monthlyRevenueData !== 'undefined') {
        destroyIfExists('monthlyRevenueChart');
        chartInstances['monthlyRevenueChart'] = new Chart(monthlyRevenueCtx, {
            type: 'bar',
            data: {
                labels: monthlyRevenueLabels,
                datasets: [{
                    label: 'Doanh thu (triệu VNĐ)',
                    data: monthlyRevenueData,
                    backgroundColor: 'rgba(102, 126, 234, 0.8)',
                    borderColor: '#667eea',
                    borderWidth: 1,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: '#f3f4f6' },
                        ticks: { color: '#6b7280' }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { color: '#6b7280' }
                    }
                }
            }
        });
    }

    // ===== 4️⃣ Biểu đồ Tỷ lệ lấp đầy =====
    const occupancyCtx = document.getElementById('occupancyChart');
    if (occupancyCtx && typeof occupancyChartlabels !== 'undefined' && typeof occupancyChartdata !== 'undefined') {
        destroyIfExists('occupancyChart');
        chartInstances['occupancyChart'] = new Chart(occupancyCtx, {
            type: 'line',
            data: {
                labels: occupancyChartlabels,
                datasets: [{
                    label: 'Tỷ lệ lấp đầy (%)',
                    data: occupancyChartdata,
                    borderColor: '#10b981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#10b981',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100,
                        grid: { color: '#f3f4f6' },
                        ticks: { color: '#6b7280' }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { color: '#6b7280' }
                    }
                }
            }
        });
    }
}
 // ✅ kết thúc đúng hàm initCharts

// Event Listeners

// Handle window resize
window.addEventListener('resize', function () {
    if (window.innerWidth >= 1024) {
        document.getElementById('sidebar').classList.remove('-translate-x-full');
        document.getElementById('overlay').classList.add('hidden');
    } else {
        document.getElementById('sidebar').classList.add('-translate-x-full');
        document.getElementById('overlay').classList.add('hidden');
    }
});

// Close modals when clicking outside
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-backdrop')) {
        const modals = document.querySelectorAll('[id$="Modal"]');
        modals.forEach(modal => {
            if (!modal.classList.contains('hidden')) {
                closeModal(modal.id);
            }
        });
    }
});