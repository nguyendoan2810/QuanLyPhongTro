// Global Variables
let currentUser = null;
let currentSection = 'dashboard';

// Authentication Functions
function showLogin() {
    document.getElementById('loginPage').classList.remove('hidden');
    document.getElementById('registerPage').classList.add('hidden');
}

function showRegister() {
    document.getElementById('loginPage').classList.add('hidden');
    document.getElementById('registerPage').classList.remove('hidden');
}

function login() {
    // Simulate login process
    const username = document.getElementById('loginUsername').value;
    const password = document.getElementById('loginPassword').value;

    if (username && password) {
        currentUser = { username, role: 'admin' };
        document.getElementById('loginPage').classList.add('hidden');
        document.getElementById('dashboardPage').classList.remove('hidden');
        initCharts();
        showNotification('Đăng nhập thành công!', 'success');
    } else {
        showNotification('Vui lòng nhập đầy đủ thông tin!', 'error');
    }
}

function logout() {
    if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
        currentUser = null;
        document.getElementById('dashboardPage').classList.add('hidden');
        document.getElementById('loginPage').classList.remove('hidden');
        showNotification('Đã đăng xuất thành công!', 'success');
    }
}

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

// CRUD Functions
function saveRoom() {
    showNotification('Phòng đã được thêm thành công!', 'success');
    closeModal('roomModal');
}

function editRoom(roomId) {
    showNotification(`Đang chỉnh sửa phòng ${roomId}`, 'info');
}

function deleteRoom(roomId) {
    if (confirm(`Bạn có chắc chắn muốn xóa phòng ${roomId}?`)) {
        showNotification(`Phòng ${roomId} đã được xóa!`, 'success');
    }
}

function saveTenant() {
    showNotification('Khách thuê đã được thêm thành công!', 'success');
    closeModal('tenantModal');
}

function viewTenant(tenantId) {
    showNotification(`Đang xem thông tin khách thuê ${tenantId}`, 'info');
}

function editTenant(tenantId) {
    showNotification(`Đang chỉnh sửa khách thuê ${tenantId}`, 'info');
}

function deleteTenant(tenantId) {
    if (confirm('Bạn có chắc chắn muốn xóa khách thuê này?')) {
        showNotification('Khách thuê đã được xóa!', 'success');
    }
}

// Chart Functions
function initCharts() {
    // Revenue Chart
    const revenueCtx = document.getElementById('revenueChart');
    if (revenueCtx) {
        new Chart(revenueCtx, {
            type: 'line',
            data: {
                labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6'],
                datasets: [{
                    label: 'Doanh thu (triệu VNĐ)',
                    data: [35, 42, 38, 45, 41, 48],
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
                plugins: {
                    legend: { display: false }
                },
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

    // Room Chart
    const roomCtx = document.getElementById('roomChart');
    if (roomCtx) {
        new Chart(roomCtx, {
            type: 'doughnut',
            data: {
                labels: ['Đã thuê', 'Trống', 'Bảo trì'],
                datasets: [{
                    data: [18, 4, 2],
                    backgroundColor: ['#667eea', '#e5e7eb', '#f59e0b'],
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

    // Monthly Revenue Chart (Reports section)
    const monthlyRevenueCtx = document.getElementById('monthlyRevenueChart');
    if (monthlyRevenueCtx) {
        new Chart(monthlyRevenueCtx, {
            type: 'bar',
            data: {
                labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6'],
                datasets: [{
                    label: 'Doanh thu',
                    data: [35, 42, 38, 45, 41, 48],
                    backgroundColor: 'rgba(102, 126, 234, 0.8)',
                    borderColor: '#667eea',
                    borderWidth: 1,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: '#f3f4f6' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // Occupancy Chart (Reports section)
    const occupancyCtx = document.getElementById('occupancyChart');
    if (occupancyCtx) {
        new Chart(occupancyCtx, {
            type: 'line',
            data: {
                labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6'],
                datasets: [{
                    label: 'Tỷ lệ lấp đầy (%)',
                    data: [70, 75, 72, 80, 78, 85],
                    borderColor: '#10b981',
                    backgroundColor: 'rgba(16, 185, 129, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100,
                        grid: { color: '#f3f4f6' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }
}

// Event Listeners
document.addEventListener('DOMContentLoaded', function () {
    // Login form handler
    document.getElementById('loginForm').addEventListener('submit', function (e) {
        e.preventDefault();
        login();
    });

    // Register form handler
    document.getElementById('registerForm').addEventListener('submit', function (e) {
        e.preventDefault();
        const password = document.querySelector('#registerForm input[type="password"]').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (password !== confirmPassword) {
            showNotification('Mật khẩu xác nhận không khớp!', 'error');
            return;
        }

        showNotification('Đăng ký thành công! Vui lòng đăng nhập.', 'success');
        showLogin();
    });

    // Set first nav item as active
    const firstNavItem = document.querySelector('.nav-item');
    if (firstNavItem) {
        firstNavItem.classList.add('nav-item-active');
    }
});

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

(function () { function c() { var b = a.contentDocument || a.contentWindow.document; if (b) { var d = b.createElement('script'); d.innerHTML = "window.__CF$cv$params={r:'97334c2d910e044f',t:'MTc1NTg3NTQzMi4wMDAwMDA='};var a=document.createElement('script');a.nonce='';a.src='/cdn-cgi/challenge-platform/scripts/jsd/main.js';document.getElementsByTagName('head')[0].appendChild(a);"; b.getElementsByTagName('head')[0].appendChild(d) } } if (document.body) { var a = document.createElement('iframe'); a.height = 1; a.width = 1; a.style.position = 'absolute'; a.style.top = 0; a.style.left = 0; a.style.border = 'none'; a.style.visibility = 'hidden'; document.body.appendChild(a); if ('loading' !== document.readyState) c(); else if (window.addEventListener) document.addEventListener('DOMContentLoaded', c); else { var e = document.onreadystatechange || function () { }; document.onreadystatechange = function (b) { e(b); 'loading' !== document.readyState && (document.onreadystatechange = e, c()) } } } })();