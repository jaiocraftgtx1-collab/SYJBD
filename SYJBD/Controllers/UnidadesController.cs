using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SYJBD.Data;
using SYJBD.Models;

namespace SYJBD.Controllers
{
    public class UnidadesController : Controller
    {
        private readonly ErpDbContext _db;
        private const int PageSize = 10;

        public UnidadesController(ErpDbContext db) => _db = db;

        // GET: /Unidades
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            var src = _db.Unidades.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                src = src.Where(x =>
                    x.IdUnidad.Contains(q) || x.Nombre.Contains(q));
                ViewData["q"] = q;
            }

            var total = await src.CountAsync();
            var items = await src
                .OrderBy(x => x.IdUnidad)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var result = new PagedResult<Unidad>
            {
                Items = items,
                Page = page,
                PageSize = PageSize,
                TotalItems = total
            };

            ViewData["Title"] = "Unidades";
            return View(result);
        }

        // GET: /Unidades/Create
        public IActionResult Create() => View(new Unidad());

        // POST: /Unidades/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Unidad vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Validación: PK duplicada
            var exists = await _db.Unidades.AnyAsync(u => u.IdUnidad == vm.IdUnidad);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.IdUnidad), "El ID ya existe.");
                return View(vm);
            }

            _db.Unidades.Add(vm);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Unidades/Edit/UND
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var entity = await _db.Unidades.FindAsync(id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Unidades/Edit/UND
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Unidad vm)
        {
            if (id != vm.IdUnidad) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            _db.Entry(vm).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Unidades/Details/UND
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var entity = await _db.Unidades.AsNoTracking().FirstOrDefaultAsync(x => x.IdUnidad == id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Unidades/Delete/UND
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();
            var entity = await _db.Unidades.FindAsync(id);
            if (entity == null) return NotFound();

            _db.Unidades.Remove(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // ---------- MODALES ----------
        // GET: /Unidades/CreateModal
        public IActionResult CreateModal()
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest") return NotFound();
            return PartialView("~/Views/Unidades/_ModalCreate.cshtml", new Unidad());
        }

        // POST: /Unidades/CreateModal
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(Unidad vm)
        {
            if (!ModelState.IsValid)
                return PartialView("~/Views/Unidades/_ModalCreate.cshtml", vm);

            var exists = await _db.Unidades.AnyAsync(u => u.IdUnidad == vm.IdUnidad);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.IdUnidad), "El ID ya existe.");
                return PartialView("~/Views/Unidades/_ModalCreate.cshtml", vm);
            }

            _db.Unidades.Add(vm);
            await _db.SaveChangesAsync();
            return NoContent(); // 204 -> JS cierra y recarga
        }

        // GET: /Unidades/EditModal/UND
        public async Task<IActionResult> EditModal(string id)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest") return NotFound();
            var entity = await _db.Unidades.FindAsync(id);
            if (entity == null) return NotFound();
            return PartialView("~/Views/Unidades/_ModalEdit.cshtml", entity);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(string id, Unidad vm)
        {
            if (id != vm.IdUnidad) return BadRequest();
            if (!ModelState.IsValid)
                return PartialView("~/Views/Unidades/_ModalEdit.cshtml", vm);

            _db.Entry(vm).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET: /Unidades/DetailsModal/UND
        public async Task<IActionResult> DetailsModal(string id)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest") return NotFound();
            var entity = await _db.Unidades.AsNoTracking().FirstOrDefaultAsync(x => x.IdUnidad == id);
            if (entity == null) return NotFound();
            return PartialView("~/Views/Unidades/_ModalDetails.cshtml", entity);
        }

        // GET: /Unidades/DeleteModal/UND
        public async Task<IActionResult> DeleteModal(string id)
        {
            if (Request.Headers["X-Requested-With"] != "XMLHttpRequest") return NotFound();
            var entity = await _db.Unidades.FindAsync(id);
            if (entity == null) return NotFound();
            return PartialView("~/Views/Unidades/_ModalDelete.cshtml", entity);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            var entity = await _db.Unidades.FindAsync(id);
            if (entity == null) return NotFound();

            try
            {
                _db.Unidades.Remove(entity);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // Si hay FK (como en tu pantallazo), devuelve mensaje claro
                ModelState.AddModelError(string.Empty, "No se puede eliminar: la unidad está en uso por otros registros.");
                return PartialView("~/Views/Unidades/_ModalDelete.cshtml", entity);
            }
        }

    }
}
