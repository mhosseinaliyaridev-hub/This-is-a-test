// Beauty Studio - Main JavaScript Application
const API_BASE_URL = 'http://localhost:5000/api';

const state = {
    services: [], specialists: [], appointments: [], customers: [], reviews: [], gallery: [],
    currentSection: 'home', bookingStep: 1, selectedService: null, selectedSpecialist: null,
    theme: localStorage.getItem('theme') || 'light', currentLightboxIndex: 0
};

document.addEventListener('DOMContentLoaded', () => { initTheme(); setupEventListeners(); loadInitialData(); hideLoadingScreen(); });

function initTheme() {
    document.documentElement.setAttribute('data-theme', state.theme);
    const toggle = document.getElementById('themeToggle');
    if (toggle) toggle.addEventListener('click', toggleTheme);
}

function toggleTheme() {
    state.theme = state.theme === 'light' ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', state.theme);
    localStorage.setItem('theme', state.theme);
}

function setupEventListeners() {
    setupNavigation();
    const mobileToggle = document.getElementById('mobileMenuToggle');
    const navMenu = document.getElementById('navMenu');
    if (mobileToggle && navMenu) mobileToggle.addEventListener('click', () => navMenu.classList.toggle('active'));
    window.addEventListener('scroll', () => {
        const navbar = document.getElementById('navbar');
        if (navbar) navbar.classList.toggle('scrolled', window.scrollY > 50);
    });
    setupServiceFilters(); setupGalleryFilters(); setupReviewForm(); setupContactForm(); setupBookingForm(); setupAdminNav();
}

function setupNavigation() {
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            navigateTo(link.getAttribute('href').substring(1));
            document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
            link.classList.add('active');
            document.getElementById('navMenu')?.classList.remove('active');
        });
    });
}

function navigateTo(section) {
    document.querySelectorAll('.section').forEach(sec => sec.classList.remove('active'));
    const target = document.getElementById(section);
    if (target) { target.classList.add('active'); window.scrollTo({ top: 0, behavior: 'smooth' }); }
    else document.getElementById('error404')?.classList.add('active');
}

async function loadInitialData() {
    try { await Promise.all([loadServices(), loadSpecialists(), loadReviews(), loadGallery()]); renderPopularServices(); renderReviewsSlider(); }
    catch (error) { showToast('Failed to load data', 'error'); }
}

async function loadServices() { try { const r = await fetch(`${API_BASE_URL}/services`); const d = await r.json(); if (d.success) state.services = d.data || []; } catch(e){} }
async function loadSpecialists() { try { const r = await fetch(`${API_BASE_URL}/specialists`); const d = await r.json(); if (d.success) state.specialists = d.data || []; } catch(e){} }
async function loadReviews() { try { const r = await fetch(`${API_BASE_URL}/reviews`); const d = await r.json(); if (d.success) { state.reviews = d.data || []; updateReviewStats(); }} catch(e){} }
async function loadGallery() { try { const r = await fetch(`${API_BASE_URL}/gallery`); const d = await r.json(); if (d.success) { state.gallery = d.data || []; renderGallery(); }} catch(e){} }

function renderPopularServices() {
    const grid = document.getElementById('popularServicesGrid'); if (!grid) return;
    grid.innerHTML = state.services.slice(0, 4).map(s => createServiceCard(s)).join('');
}

function createServiceCard(s) {
    const icons = { Hair:'💇', Skin:'✨', Nails:'💅', Makeup:'💄', Spa:'🧖', Massage:'💆' };
    return `<div class="service-card" onclick="showServiceDetail('${s.id}')"><div class="service-image">${icons[s.category]||'💫'}</div><div class="service-content"><h3>${s.name}</h3><p>${s.description}</p><div class="service-meta"><span>$${s.price}</span><span>${s.durationMinutes}min</span></div></div></div>`;
}

function renderServices(services=state.services) {
    const list = document.getElementById('servicesList'), empty = document.getElementById('servicesEmpty'); if(!list)return;
    if(!services.length){list.innerHTML='';empty?.classList.remove('hidden');}else{empty?.classList.add('hidden');list.innerHTML=services.map(s=>createServiceCard(s)).join('');}
}

function renderSpecialists(specialists=state.specialists) {
    const grid = document.getElementById('specialistsGrid'), empty = document.getElementById('specialistsEmpty'); if(!grid)return;
    if(!specialists.length){grid.innerHTML='';empty?.classList.remove('hidden');}else{empty?.classList.add('hidden');grid.innerHTML=specialists.map(sp=>`<div class="specialist-card"><div class="specialist-image">👤</div><h3>${sp.firstName} ${sp.lastName}</h3><p>${sp.title}</p><div>${'★'.repeat(Math.floor(sp.rating))}${'☆'.repeat(5-Math.floor(sp.rating))}</div><p>${sp.experienceYears}y exp</p></div>`).join('');}
}

function renderReviewsSlider() { const s=document.getElementById('reviewsSlider');if(!s)return;s.innerHTML=state.reviews.slice(0,3).map(r=>createReviewCard(r)).join(''); }
function renderReviewsGrid() { const g=document.getElementById('reviewsGrid');if(!g)return;g.innerHTML=state.reviews.map(r=>createReviewCard(r)).join(''); }
function createReviewCard(r) { const i=r.customerName.split(' ').map(n=>n[0]).join('').toUpperCase(); return `<div class="review-card"><div class="review-header"><div class="review-avatar">${i}</div><div><h4>${r.customerName}</h4><div>${'★'.repeat(r.rating)}${'☆'.repeat(5-r.rating)}</div></div></div><p>"${r.comment}"</p></div>`; }

function renderGallery(cat='') {
    const g=document.getElementById('galleryGrid');if(!g)return;
    const f=cat?state.gallery.filter(i=>i.category===cat):state.gallery;
    g.innerHTML=f.map((img,idx)=>`<div class="gallery-item" onclick="openLightbox(${state.gallery.indexOf(img)})"><img src="${img.imageUrl}" alt="${img.title}"><div class="gallery-overlay"><h4>${img.title}</h4></div></div>`).join('');
}

function setupServiceFilters() {
    const si=document.getElementById('serviceSearch'),cf=document.getElementById('serviceCategoryFilter'),sf=document.getElementById('serviceSort');
    if(si)si.addEventListener('input',filterServices);if(cf)cf.addEventListener('change',filterServices);if(sf)sf.addEventListener('change',filterServices);
}

function filterServices() {
    const s=document.getElementById('serviceSearch')?.value.toLowerCase()||'',c=document.getElementById('serviceCategoryFilter')?.value||'',so=document.getElementById('serviceSort')?.value||'name';
    let f=state.services;if(s)f=f.filter(x=>x.name.toLowerCase().includes(s)||x.description.toLowerCase().includes(s));if(c)f=f.filter(x=>x.category===c);
    if(so==='price-low')f.sort((a,b)=>a.price-b.price);else if(so==='price-high')f.sort((a,b)=>b.price-a.price);else if(so==='duration')f.sort((a,b)=>a.durationMinutes-b.durationMinutes);else f.sort((a,b)=>a.name.localeCompare(b.name));
    renderServices(f);
}

function setupGalleryFilters() { document.querySelectorAll('.gallery-filter-btn').forEach(b=>b.addEventListener('click',()=>{document.querySelectorAll('.gallery-filter-btn').forEach(x=>x.classList.remove('active'));b.classList.add('active');renderGallery(b.dataset.category);})); }

function openLightbox(i) { state.currentLightboxIndex=i;const lb=document.getElementById('lightbox'),img=document.getElementById('lightboxImage'),cap=document.getElementById('lightboxCaption');if(lb&&img&&state.gallery[i]){img.src=state.gallery[i].imageUrl;cap.textContent=state.gallery[i].title;lb.classList.add('active');} }
function closeLightbox() { document.getElementById('lightbox')?.classList.remove('active'); }
function previousImage() { state.currentLightboxIndex=(state.currentLightboxIndex-1+state.gallery.length)%state.gallery.length;openLightbox(state.currentLightboxIndex); }
function nextImage() { state.currentLightboxIndex=(state.currentLightboxIndex+1)%state.gallery.length;openLightbox(state.currentLightboxIndex); }

function updateReviewStats() {
    const ar=document.getElementById('averageRating'),as=document.getElementById('averageStars'),tr=document.getElementById('totalReviews');
    if(state.reviews.length){const a=state.reviews.reduce((s,r)=>s+r.rating,0)/state.reviews.length;if(ar)ar.textContent=a.toFixed(1);if(as)as.innerHTML='★'.repeat(Math.floor(a))+'☆'.repeat(5-Math.floor(a));if(tr)tr.textContent=`${state.reviews.length} reviews`;}
}

function openReviewModal() { document.getElementById('reviewModal')?.classList.add('active'); }
function closeReviewModal() { document.getElementById('reviewModal')?.classList.remove('active'); }

function setupReviewForm() {
    const sr=document.getElementById('starRating'),rv=document.getElementById('reviewRatingValue');
    if(sr)sr.querySelectorAll('span').forEach(st=>st.addEventListener('click',()=>{const r=parseInt(st.dataset.rating);rv.value=r;sr.querySelectorAll('span').forEach((x,i)=>x.classList.toggle('active',i<r));}));
    const f=document.getElementById('reviewForm');if(f)f.addEventListener('submit',async(e)=>{e.preventDefault();const review={customerName:document.getElementById('reviewName').value,rating:parseInt(document.getElementById('reviewRatingValue').value),comment:document.getElementById('reviewComment').value};try{const r=await fetch(`${API_BASE_URL}/reviews`,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(review)});const d=await r.json();if(d.success){showToast('Review submitted!','success');closeReviewModal();f.reset();loadReviews();}else showToast(d.message||'Failed','error');}catch(err){showToast('Failed','error');}});
}

function setupContactForm() { const f=document.getElementById('contactForm');if(f)f.addEventListener('submit',(e)=>{e.preventDefault();showToast('Message sent!','success');f.reset();}); }
function setupBookingForm() { const di=document.getElementById('appointmentDate');if(di){di.min=new Date().toISOString().split('T')[0];di.addEventListener('change',handleDateChange);} }

async function handleDateChange() { const d=document.getElementById('appointmentDate').value,sid=document.getElementById('selectedSpecialistId').value;if(d&&sid)await loadTimeSlots(d,sid); }
async function loadTimeSlots(date,sid) {
    const c=document.getElementById('timeSlots');if(!c)return;
    try{const r=await fetch(`${API_BASE_URL}/appointments?date=${date}`),d=await r.json(),apts=d.data||[];const bt=apts.filter(a=>a.specialistId===sid&&a.status!=='Cancelled').map(a=>a.appointmentTime);let slots=[];for(let h=9;h<18;h++)for(let m of[0,15,30,45]){const t=`${h.toString().padStart(2,'0')}:${m.toString().padStart(2,'0')}`,ib=bt.some(x=>x.startsWith(t));slots.push({t,av:!ib});}c.innerHTML=slots.map(sl=>`<div class="time-slot${sl.av?'':' unavailable'}"onclick="selectTimeSlot('${sl.t}',${sl.av})">${sl.t}</div>`).join('');}catch(e){console.error(e);}
}

function selectTimeSlot(t,av) { if(!av)return;document.querySelectorAll('.time-slot').forEach(s=>s.classList.remove('selected'));event.target.classList.add('selected');document.getElementById('selectedTime').value=t; }

let currentBookingStep=1;
function nextBookingStep() { if(!validateBookingStep(currentBookingStep))return;currentBookingStep++;updateBookingSteps();if(currentBookingStep===2)renderBookingSpecialists();else if(currentBookingStep===5)renderBookingSummary(); }
function prevBookingStep() { currentBookingStep--;updateBookingSteps(); }
function updateBookingSteps() {
    document.querySelectorAll('.booking-step').forEach((s,i)=>{s.classList.toggle('active',i+1===currentBookingStep);s.classList.toggle('completed',i+1<currentBookingStep);});
    document.querySelectorAll('.booking-step-content').forEach((c,i)=>c.classList.toggle('active',i+1===currentBookingStep));
    document.getElementById('prevStepBtn').disabled=currentBookingStep===1;document.getElementById('nextStepBtn').classList.toggle('hidden',currentBookingStep===5);document.getElementById('submitBookingBtn').classList.toggle('hidden',currentBookingStep!==5);
}

function validateBookingStep(step) {
    if(step===1&&!document.getElementById('selectedServiceId').value){showToast('Select a service','warning');return false;}
    if(step===2&&!document.getElementById('selectedSpecialistId').value){showToast('Select a specialist','warning');return false;}
    if(step===3&&(!document.getElementById('appointmentDate').value||!document.getElementById('selectedTime').value)){showToast('Select date and time','warning');return false;}
    if(step===4){const fn=document.getElementById('customerFirstName').value,ln=document.getElementById('customerLastName').value,em=document.getElementById('customerEmail').value,ph=document.getElementById('customerPhone').value;if(!fn||!ln||!em||!ph){showToast('Fill required fields','warning');return false;}}
    return true;
}

function renderBookingServices() { const g=document.getElementById('bookingServicesGrid');if(!g)return;g.innerHTML=state.services.map(s=>`<div class="selection-card"onclick="selectBookingService('${s.id}')"><h4>${s.name}</h4><p>$${s.price}-${s.durationMinutes}min</p></div>`).join(''); }
function selectBookingService(id) { state.selectedService=state.services.find(s=>s.id===id);document.getElementById('selectedServiceId').value=id;document.querySelectorAll('.selection-card').forEach(c=>c.classList.remove('selected'));event.target.closest('.selection-card').classList.add('selected'); }
function renderBookingSpecialists() { const g=document.getElementById('bookingSpecialistsGrid');if(!g)return;g.innerHTML=state.specialists.map(s=>`<div class="selection-card"onclick="selectBookingSpecialist('${s.id}')"><h4>${s.firstName} ${s.lastName}</h4><p>${s.title}</p></div>`).join(''); }
function selectBookingSpecialist(id) { state.selectedSpecialist=state.specialists.find(s=>s.id===id);document.getElementById('selectedSpecialistId').value=id;document.querySelectorAll('.selection-card').forEach(c=>c.classList.remove('selected'));event.target.closest('.selection-card').classList.add('selected');const d=document.getElementById('appointmentDate').value;if(d)loadTimeSlots(d,id); }
function renderBookingSummary() { const s=document.getElementById('bookingSummary');if(!s)return;const sv=state.selectedService,sp=state.selectedSpecialist,d=document.getElementById('appointmentDate').value,t=document.getElementById('selectedTime').value;s.innerHTML=`<h4>Service:</h4><p>${sv?.name}-$${sv?.price}</p><h4>Specialist:</h4><p>${sp?.firstName} ${sp?.lastName}</p><h4>Date&Time:</h4><p>${new Date(d).toLocaleDateString()} at ${t}</p><h4>Customer:</h4><p>${document.getElementById('customerFirstName').value} ${document.getElementById('customerLastName').value}</p>`; }

document.getElementById('bookingForm')?.addEventListener('submit',async(e)=>{e.preventDefault();const apt={customerId:'temp-'+Date.now(),serviceId:document.getElementById('selectedServiceId').value,specialistId:document.getElementById('selectedSpecialistId').value,appointmentDate:document.getElementById('appointmentDate').value,appointmentTime:document.getElementById('selectedTime').value+':00',notes:document.getElementById('customerNotes').value,status:'Pending'};try{const r=await fetch(`${API_BASE_URL}/appointments`,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(apt)});const d=await r.json();if(d.success)showConfirmation(d.data);else showToast(d.message||'Failed','error');}catch(err){showToast('Failed','error');}});

function showConfirmation(apt) { document.querySelectorAll('.section').forEach(s=>s.classList.remove('active'));document.getElementById('bookingConfirmation')?.classList.remove('hidden');document.getElementById('bookingConfirmation')?.classList.add('active');document.getElementById('confirmationCode').textContent=apt.reservationCode;document.getElementById('confirmationDetails').innerHTML=`<p><strong>Service:</strong>${apt.service?.name||'N/A'}</p><p><strong>Specialist:</strong>${apt.specialist?.firstName||'N/A'}</p><p><strong>Date:</strong>${new Date(apt.appointmentDate).toLocaleDateString()}</p><p><strong>Time:</strong>${apt.appointmentTime}</p>`; }

function setupAdminNav() { document.querySelectorAll('.admin-nav-btn').forEach(b=>b.addEventListener('click',()=>{document.querySelectorAll('.admin-nav-btn').forEach(x=>x.classList.remove('active'));b.classList.add('active');document.querySelectorAll('.admin-tab').forEach(t=>t.classList.remove('active'));document.getElementById(`${b.dataset.tab}Tab`)?.classList.add('active');loadAdminData(b.dataset.tab);})); }
async function loadAdminData(tab) { if(tab==='dashboard')loadDashboardStats();else if(tab==='customers')loadAdminCustomers();else if(tab==='appointments')loadAdminAppointments();else if(tab==='services')loadAdminServices();else if(tab==='specialists')loadAdminSpecialists(); }

async function loadDashboardStats() { try{const r=await fetch(`${API_BASE_URL}/dashboard/stats`),d=await r.json();if(d.success&&d.data){const s=d.data;document.getElementById('dashTotalCustomers').textContent=s.totalCustomers||0;document.getElementById('dashTotalAppointments').textContent=s.totalAppointments||0;document.getElementById('dashTodayAppointments').textContent=s.todayAppointments||0;document.getElementById('dashCompletedAppointments').textContent=s.completedAppointments||0;document.getElementById('dashCancelledAppointments').textContent=s.cancelledAppointments||0;document.getElementById('dashRevenue').textContent=`$${s.totalRevenue||0}`;}}catch(e){} }

async function loadAdminCustomers() { try{const r=await fetch(`${API_BASE_URL}/customers`),d=await r.json();if(d.success){const tb=document.getElementById('customersTableBody');if(tb)tb.innerHTML=(d.data||[]).map(c=>`<tr><td>${c.firstName} ${c.lastName}</td><td>${c.email}</td><td>${c.phoneNumber}</td><td>-</td><td><button class="btn btn-secondary"style="padding:4px 8px;font-size:0.8rem">Edit</button></td></tr>`).join('');}}catch(e){} }
async function loadAdminAppointments() { try{const r=await fetch(`${API_BASE_URL}/appointments`),d=await r.json();if(d.success){const tb=document.getElementById('appointmentsTableBody');if(tb)tb.innerHTML=(d.data||[]).map(a=>`<tr><td>${a.reservationCode}</td><td>${a.customer?.firstName||'N/A'}</td><td>${a.service?.name||'N/A'}</td><td>${a.specialist?.firstName||'N/A'}</td><td>${new Date(a.appointmentDate).toLocaleDateString()} ${a.appointmentTime}</td><td><span class="status-badge status-${a.status.toLowerCase()}">${a.status}</span></td><td><button class="btn btn-secondary"style="padding:4px 8px;font-size:0.8rem"onclick="updateAppointmentStatus('${a.id}','Confirmed')">Confirm</button></td></tr>`).join('');}}catch(e){} }
async function updateAppointmentStatus(id,st) { try{const r=await fetch(`${API_BASE_URL}/appointments/${id}/status`,{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify(st)});const d=await r.json();if(d.success){showToast(`Appointment ${st}`,'success');loadAdminAppointments();}else showToast(d.message||'Failed','error');}catch(e){showToast('Failed','error');} }
async function loadAdminServices() { try{const r=await fetch(`${API_BASE_URL}/services`),d=await r.json();if(d.success){const tb=document.getElementById('servicesTableBody');if(tb)tb.innerHTML=(d.data||[]).map(s=>`<tr><td>${s.name}</td><td>${s.category}</td><td>$${s.price}</td><td>${s.durationMinutes}min</td><td><button class="btn btn-secondary"style="padding:4px 8px;font-size:0.8rem">Edit</button></td></tr>`).join('');}}catch(e){} }
async function loadAdminSpecialists() { try{const r=await fetch(`${API_BASE_URL}/specialists`),d=await r.json();if(d.success){const tb=document.getElementById('specialistsTableBody');if(tb)tb.innerHTML=(d.data||[]).map(s=>`<tr><td>${s.firstName} ${s.lastName}</td><td>${s.title}</td><td>${s.experienceYears}y</td><td>★${s.rating}</td><td><button class="btn btn-secondary"style="padding:4px 8px;font-size:0.8rem">Edit</button></td></tr>`).join('');}}catch(e){} }

function showToast(msg,type='info') { const c=document.getElementById('toastContainer');if(!c)return;const t=document.createElement('div');t.className=`toast ${type}`;t.textContent=msg;c.appendChild(t);setTimeout(()=>{t.style.animation='slideIn 0.3s reverse';setTimeout(()=>t.remove(),300);},3000); }
function hideLoadingScreen() { const s=document.getElementById('loadingScreen');if(s){s.style.opacity='0';setTimeout(()=>s.remove(),500);} }

window.closeServiceModal=()=>document.getElementById('serviceModal')?.classList.remove('active');
window.showServiceDetail=(id)=>{const s=state.services.find(x=>x.id===id);if(!s)return;const m=document.getElementById('serviceModal'),c=document.getElementById('serviceModalContent');if(m&&c){c.innerHTML=`<h2>${s.name}</h2><p>${s.description}</p><p><strong>Price:</strong>$${s.price}</p><p><strong>Duration:</strong>${s.durationMinutes}min</p><button class="btn btn-primary"onclick="navigateTo('booking');closeServiceModal();renderBookingServices()">Book Now</button>`;m.classList.add('active');}};
window.openReviewModal=openReviewModal;window.closeReviewModal=closeReviewModal;window.closeLightbox=closeLightbox;window.previousImage=previousImage;window.nextImage=nextImage;window.prevBookingStep=prevBookingStep;window.nextBookingStep=nextBookingStep;window.selectTimeSlot=selectTimeSlot;window.navigateTo=navigateTo;
