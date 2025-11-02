// ======== HÀM THÔNG BÁO TOÀN CỤC (Notification) ========
function showNotification(message, type = 'info') {
    // Tạo phần tử thông báo
    const notification = document.createElement('div');
    notification.className = `
        fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg max-w-sm bounce-in
        ${type === 'success' ? 'bg-green-500 text-white' :
            type === 'error' ? 'bg-red-500 text-white' :
                type === 'warning' ? 'bg-yellow-500 text-white' :
                    'bg-blue-500 text-white'}
    `;

    // Nội dung
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

    // Tự động biến mất sau 5 giây
    setTimeout(() => {
        if (notification.parentElement) notification.remove();
    }, 5000);
}

// khachthue.js - quản lý onepage KhachThue
document.addEventListener("DOMContentLoaded", () => {
    // Element references (an toàn nếu không tồn tại)
    const mainApp = document.getElementById("mainApp");
    const sidebar = document.getElementById("sidebar");
    const overlay = document.getElementById("sidebarOverlay");
    const sidebarToggle = document.getElementById("sidebarToggle");

    // === Helper: an toàn khi query selector (tránh lỗi nếu element không có) ===
    const q = sel => document.querySelector(sel);
    const qa = sel => Array.from(document.querySelectorAll(sel));

    // === Sidebar mobile toggle ===
    sidebarToggle?.addEventListener("click", () => {
        sidebar?.classList.toggle("-translate-x-full");
        overlay?.classList.toggle("hidden");
    });

    overlay?.addEventListener("click", () => {
        sidebar?.classList.add("-translate-x-full");
        overlay?.classList.add("hidden");
    });

    // === showPage: ẩn tất cả page-content và hiển thị page được chọn ===
    function showPage(pageId) {
        qa(".page-content").forEach(p => {
            p.classList.add("hidden");
            p.classList.remove("fade-in");
        });

        const page = document.getElementById(`${pageId}Page`);
        if (page) {
            page.classList.remove("hidden");
            // trigger reflow để animation hoạt động nếu cần
            void page.offsetWidth;
            page.classList.add("fade-in");
        }

        // cập nhật trạng thái active trên sidebar
        qa(".nav-item").forEach(item => {
            item.classList.remove("bg-blue-50", "text-blue-600");
            item.classList.add("text-gray-700");
        });
        const activeItem = q(`[data-page="${pageId}"]`);
        if (activeItem) {
            activeItem.classList.add("bg-blue-50", "text-blue-600");
            activeItem.classList.remove("text-gray-700");
        }

        // đóng sidebar mobile nếu mở
        sidebar?.classList.add("-translate-x-full");
        overlay?.classList.add("hidden");
    }

    // ==== Gán sự kiện cho nav items (nếu có) ====
    qa(".nav-item").forEach(item => {
        item.addEventListener("click", function () {
            const pageId = this.getAttribute("data-page");
            if (pageId) showPage(pageId);
        });
    });

    // ==== Close modal khi click backdrop ====
    document.addEventListener("click", e => {
        if (e.target.classList && e.target.classList.contains("modal-backdrop")) {
            qa(".modal-backdrop").forEach(m => {
                m.classList.add("hidden");
                m.classList.remove("flex");
            });
        }
    });

    // ==== Khi vào KhachThueMain/Index (mainApp có trong DOM) ====
    if (mainApp) {
        // Mặc định hiển thị dashboard
        showPage("dashboard");

        // (TÙY CHỌN) nếu muốn nhớ trang đang mở giữa các lần reload
        const saved = localStorage.getItem("kh_page");
        if (saved) showPage(saved);
        qa(".nav-item").forEach(i => i.addEventListener('click', () => localStorage.setItem('kh_page', i.dataset.page)));
    }
});