// Sample data and state management
let currentUser = null;
let isEditingProfile = false;

// Login functionality
document.getElementById('loginForm').addEventListener('submit', function (e) {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    // Simple validation (in real app, this would be server-side)
    if (username && password) {
        currentUser = { username: username, name: 'Nguyễn Văn A' };
        document.getElementById('loginPage').classList.add('hidden');
        document.getElementById('mainApp').classList.remove('hidden');
        showPage('dashboard');
    } else {
        alert('Vui lòng nhập đầy đủ thông tin đăng nhập');
    }
});

// Logout functionality
document.getElementById('logoutBtn').addEventListener('click', function () {
    currentUser = null;
    document.getElementById('mainApp').classList.add('hidden');
    document.getElementById('loginPage').classList.remove('hidden');
    document.getElementById('username').value = '';
    document.getElementById('password').value = '';
});

// Sidebar toggle for mobile
document.getElementById('sidebarToggle').addEventListener('click', function () {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    sidebar.classList.toggle('-translate-x-full');
    overlay.classList.toggle('hidden');
});

// Close sidebar when clicking overlay
document.getElementById('sidebarOverlay').addEventListener('click', function () {
    document.getElementById('sidebar').classList.add('-translate-x-full');
    this.classList.add('hidden');
});

// Navigation functionality
function showPage(pageId) {
    // Hide all pages
    document.querySelectorAll('.page-content').forEach(page => {
        page.classList.add('hidden');
    });

    // Show selected page
    document.getElementById(pageId + 'Page').classList.remove('hidden');
    document.getElementById(pageId + 'Page').classList.add('fade-in');

    // Update navigation active state
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('bg-blue-50', 'text-blue-600');
        item.classList.add('text-gray-700');
    });

    document.querySelector(`[data-page="${pageId}"]`).classList.add('bg-blue-50', 'text-blue-600');
    document.querySelector(`[data-page="${pageId}"]`).classList.remove('text-gray-700');

    // Close mobile sidebar
    document.getElementById('sidebar').classList.add('-translate-x-full');
    document.getElementById('sidebarOverlay').classList.add('hidden');
}

// Add navigation event listeners
document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', function () {
        const pageId = this.getAttribute('data-page');
        showPage(pageId);
    });
});

// Profile editing functionality
document.getElementById('editProfileBtn').addEventListener('click', function () {
    const inputs = document.querySelectorAll('#profileForm input');
    const actions = document.getElementById('profileActions');

    inputs.forEach(input => input.disabled = false);
    actions.classList.remove('hidden');
    isEditingProfile = true;
});

document.getElementById('cancelEditBtn').addEventListener('click', function () {
    const inputs = document.querySelectorAll('#profileForm input');
    const actions = document.getElementById('profileActions');

    inputs.forEach(input => input.disabled = true);
    actions.classList.add('hidden');
    isEditingProfile = false;
});

document.getElementById('saveProfileBtn').addEventListener('click', function () {
    const inputs = document.querySelectorAll('#profileForm input');
    const actions = document.getElementById('profileActions');

    inputs.forEach(input => input.disabled = true);
    actions.classList.add('hidden');
    isEditingProfile = false;

    alert('Thông tin cá nhân đã được cập nhật thành công!');
});

// Contract modal functionality
document.querySelectorAll('.view-contract-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        document.getElementById('contractModal').classList.remove('hidden');
        document.getElementById('contractModal').classList.add('flex');
    });
});

document.getElementById('closeContractModal').addEventListener('click', function () {
    document.getElementById('contractModal').classList.add('hidden');
    document.getElementById('contractModal').classList.remove('flex');
});

// Contract actions
document.querySelectorAll('.extend-contract-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        alert('Yêu cầu gia hạn hợp đồng đã được gửi. Chủ trọ sẽ liên hệ với bạn sớm.');
    });
});

document.querySelectorAll('.terminate-contract-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        if (confirm('Bạn có chắc chắn muốn yêu cầu chấm dứt hợp đồng?')) {
            alert('Yêu cầu chấm dứt hợp đồng đã được gửi. Chủ trọ sẽ liên hệ với bạn để xử lý.');
        }
    });
});

// Bill tabs functionality
document.querySelectorAll('.bill-tab-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        const tabId = this.getAttribute('data-tab');

        // Update tab buttons
        document.querySelectorAll('.bill-tab-btn').forEach(b => {
            b.classList.remove('bg-blue-600', 'text-white');
            b.classList.add('bg-gray-200', 'text-gray-700');
        });
        this.classList.add('bg-blue-600', 'text-white');
        this.classList.remove('bg-gray-200', 'text-gray-700');

        // Update tab content
        document.querySelectorAll('.bill-tab-content').forEach(content => {
            content.classList.add('hidden');
        });

        if (tabId === 'current') {
            document.getElementById('currentBills').classList.remove('hidden');
        } else {
            document.getElementById('historyBills').classList.remove('hidden');
        }
    });
});

// Bill modal functionality
document.querySelectorAll('.view-bill-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        document.getElementById('billModal').classList.remove('hidden');
        document.getElementById('billModal').classList.add('flex');
    });
});

document.getElementById('closeBillModal').addEventListener('click', function () {
    document.getElementById('billModal').classList.add('hidden');
    document.getElementById('billModal').classList.remove('flex');
});

// Payment functionality
document.querySelectorAll('.pay-bill-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        alert('Chức năng thanh toán online đang được phát triển. Vui lòng thanh toán trực tiếp hoặc chuyển khoản.');
    });
});

// Contact form functionality
document.getElementById('contactForm').addEventListener('submit', function (e) {
    e.preventDefault();
    alert('Yêu cầu của bạn đã được gửi thành công! Chúng tôi sẽ phản hồi trong thời gian sớm nhất.');
    this.reset();
});

// Close modals when clicking outside
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-backdrop')) {
        document.querySelectorAll('.modal-backdrop').forEach(modal => {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
        });
    }
});

// Initialize app
document.addEventListener('DOMContentLoaded', function () {
    // Show login page by default
    document.getElementById('loginPage').classList.remove('hidden');
});