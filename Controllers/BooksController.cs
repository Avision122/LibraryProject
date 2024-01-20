using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projekt_studia2.Models;
using Projekt_studia2.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Projekt_studia2.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;
        private readonly LoggingService _loggingService;

        public BooksController(LibraryContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }
        public async Task<IActionResult> Index(string query)
        {
            var viewModel = new BooksViewModel();

            if (!string.IsNullOrEmpty(query))
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja Index zakonczona pomyslnie");
                viewModel.Books = await _context.Books
                    .Where(book => book.Title.Contains(query) || book.Author.Contains(query))
                    .ToListAsync();
            }
            else
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja Index zakonczona pomyslnie");
                viewModel.Books = await _context.Books.ToListAsync();
            }

            return View(viewModel);
        }
        public async Task<IActionResult> Search(string query)
        {
            var viewModel = new BooksViewModel
            {
                Books = await _context.Books
                    .Where(book => book.Title.Contains(query) || book.Author.Contains(query))
                    .ToListAsync()
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Rent()
        {
            var availableBooks = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .Select(b => new SelectListItem
                {
                    Value = b.ID.ToString(),
                    Text = b.Title
                }).ToListAsync();

            ViewBag.AvailableBooks = new SelectList(availableBooks, "Value", "Text");
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Rent(int bookId)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.ID == bookId);
            var userLogin = User.Identity.Name;

            if (book == null || book.AvailableCopies <= 0)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Książka nie jest już dostępna w akcji Rent!");
                TempData["ErrorMessage"] = "Książka nie jest dostępna.";
                return RedirectToAction("Index");
            }

            var currentRentalsCount = await _context.RentalQueues.CountAsync(r => r.UserLogin == userLogin);
            if (currentRentalsCount >= 3)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja Rent osiągneła limit 3 próśb wypożyczenia!");
                TempData["ErrorMessage"] = "Osiągnięto limit maksymalnie trzech próśb wypożyczenia.";
                return RedirectToAction("Index");
            }

            var alreadyRequested = await _context.RentalQueues.AnyAsync(r => r.UserLogin == userLogin && r.BookID == bookId);
            if (alreadyRequested)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja Rent zakończona niepowodzeniem - prośba o wypożyczenie tej książki już istnieje!");
                TempData["ErrorMessage"] = "Już złożono prośbę o wypożyczenie tej książki.";
                return RedirectToAction("Index");
            }

            var rentalQueue = new RentalQueue
            {
                BookID = book.ID,
                BookTitle = book.Title,
                UserLogin = userLogin
            };

            _context.RentalQueues.Add(rentalQueue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Prośba o wypożyczenie książki została złożona.";
            var availableBooks = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .Select(b => new SelectListItem
                {
                    Value = b.ID.ToString(),
                    Text = b.Title
                }).ToListAsync();
            await _loggingService.LogEvent(User.Identity.Name, "Wykonał akcję Rent");
            ViewBag.AvailableBooks = new SelectList(availableBooks, "Value", "Text");
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpGet]
        public IActionResult Add()
        {
            return View(new AddBookViewModel());
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpPost]
        public async Task<IActionResult> Add(AddBookViewModel model)
        {
            if (ModelState.IsValid)
            {
                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Year = model.Year,
                    AvailableCopies = model.AvailableCopies
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                await _loggingService.LogEvent(User.Identity.Name, "Wykonał akcję Add!");
                TempData["SuccessMessage"] = "Książka została pomyślnie dodana.";
                return RedirectToAction("Add");
            }

            await _loggingService.LogEvent(User.Identity.Name, "Akcja Add zakonczona niepowodzeniem");
            TempData["ErrorMessage"] = "Wystąpił błąd podczas dodawania książki.";
            return View(model);
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpGet]
        public async Task<IActionResult> ManageRentals()
        {
            var rentalRequests = await _context.RentalQueues.ToListAsync();
            return View(rentalRequests); 
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpPost]
        public async Task<IActionResult> AcceptRental(int requestId)
        {
            var rentalRequest = await _context.RentalQueues.FindAsync(requestId);
            if (rentalRequest != null)
            {
                var book = await _context.Books.FindAsync(rentalRequest.BookID);
                if (book != null && book.AvailableCopies > 0)
                {
                    book.AvailableCopies--;

                    var rental = new Rental
                    {
                        UserLogin = rentalRequest.UserLogin,
                        BookID = rentalRequest.BookID,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddDays(14),
                        BookTitle = rentalRequest.BookTitle
                    };
                    _context.Rentals.Add(rental);

                    _context.RentalQueues.Remove(rentalRequest);

                    await _context.SaveChangesAsync();
                    await _loggingService.LogEvent(User.Identity.Name, "Akcja AcceptRental została pomyślnie wykonana!");
                    TempData["SuccessMessage"] = $"Wypożyczenie książki '{book.Title}' zostało pomyślnie zaakceptowane.";
                    return RedirectToAction("ManageRentals");
                }
                else
                {
                    await _loggingService.LogEvent(User.Identity.Name, "Akcja AcceptRental zakończona niepowodzeniem - brak dostępnych kopii");
                    TempData["ErrorMessage"] = "Nie można zaakceptować wypożyczenia. Brak dostępnych kopii książki.";
                    return RedirectToAction("ManageRentals");
                }
            }
            else
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja AcceptRental zakończona niepowodzeniem - nie znaleziono prośby o wypozyczenie");
                TempData["ErrorMessage"] = "Nie znaleziono prośby o wypożyczenie.";
                return RedirectToAction("ManageRentals");
            }
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpPost]
        public async Task<IActionResult> RejectRental(int requestId)
        {
            var request = await _context.RentalQueues.FindAsync(requestId);
            if (request != null)
            {
                _context.RentalQueues.Remove(request);
                await _context.SaveChangesAsync();
                await _loggingService.LogEvent(User.Identity.Name, "Akcja RejectRental zakończona pomyślnie!");
                TempData["SuccessMessage"] = "Prośba o wypożyczenie książki została odrzucona.";
            }
            else
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja RejectRental zakończona niepowodzeniem - nie znaleziono prośby o wypożyczenie");
                TempData["ErrorMessage"] = "Nie znaleziono prośby o wypożyczenie.";
            }

            return RedirectToAction("ManageRentals");
        }

        public async Task<IActionResult> MyRentals()
        {
            var userLogin = User.Identity.Name;
            var rentals = await _context.Rentals
                .Where(r => r.UserLogin == userLogin)
                .Select(r => new RentalViewModel
                {
                    RentalId = r.ID,
                    BookTitle = r.BookTitle,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate
                })
                .ToListAsync();
            await _loggingService.LogEvent(User.Identity.Name, "Akcja MyRental zakończona powodzeniem");
            return View(rentals);
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpGet]
        public async Task<IActionResult> DisplayUserRentals()
        {
            var users = await _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Username,
                    Text = u.Username
                })
                .ToListAsync();

            var viewModel = new UserRentalsViewModel
            {
                Users = users,
                Rentals = new List<RentalDetails>()
            };

            return View(viewModel);
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpPost]
        public async Task<IActionResult> DisplayUserRentals(UserRentalsViewModel viewModel)
        {
            if (!String.IsNullOrWhiteSpace(viewModel.SelectedUsername))
            {
                viewModel.Rentals = await _context.Rentals
                    .Where(r => r.UserLogin == viewModel.SelectedUsername)
                    .Select(r => new RentalDetails
                    {
                        BookTitle = r.BookTitle,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate
                    })
                    .ToListAsync();
            }

            await _loggingService.LogEvent(User.Identity.Name, "Akcja DisplayUserRentals zakonczona powodzeniem");
            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> ReturnBook()
        {
            var userLogin = User.Identity.Name;
            var userRentals = await _context.Rentals
                                            .Where(r => r.UserLogin == userLogin)
                                            .ToListAsync();

            var model = new ReturnBookViewModel
            {
                Rentals = userRentals.Select(r => new SelectListItem
                {
                    Value = r.ID.ToString(),
                    Text = r.BookTitle
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook(ReturnQueue returnRequest)
        {
            returnRequest.RequestDate = DateTime.Now;

            var book = await _context.Books.FirstOrDefaultAsync(b => b.ID == returnRequest.BookId);
            if (book == null)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja ReturnBook zakonczona niepowodzeniem - ksiazka nie istnieje");
                return View(returnRequest);
            }

            _context.ReturnQueues.Add(returnRequest);
            await _context.SaveChangesAsync();

            await _loggingService.LogEvent(User.Identity.Name, "Akcja ReturnBook zakonczona pomyślnie - prośba o zwrot książki została złożona ");
            TempData["SuccessMessage"] = "Prośba o zwrot książki została złożona.";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ProcessReturn(ReturnBookViewModel model)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.ID == model.SelectedRentalId);
            if (rental == null)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja ProcessReturn zakonczona niepowodzeniem - nie znaleziono wypożyczenia");
                TempData["ErrorMessage"] = "Nie znaleziono wypożyczenia.";
                return RedirectToAction("ReturnBook");
            }

            var existingReturnRequest = await _context.ReturnQueues
                .AnyAsync(rr => rr.BookId == rental.BookID && rr.UserLogin == rental.UserLogin);
            if (existingReturnRequest)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja ProcessReturn zakonczona niepowodzeniem - prośba o zwrot tej książki została już złożona");
                TempData["ErrorMessage"] = "Prośba o zwrot tej książki została już złożona.";
                return RedirectToAction("ReturnBook");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.ID == rental.BookID);
            if (book == null)
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja ProcessReturn zakonczona niepowodzeniem - nie znaleziono książki");
                TempData["ErrorMessage"] = "Nie znaleziono książki.";
                return RedirectToAction("ReturnBook");
            }

            var returnRequest = new ReturnQueue
            {
                BookId = rental.BookID,
                BookTitle = book.Title,
                UserLogin = rental.UserLogin,
                RequestDate = DateTime.Now
            };
            
            _context.ReturnQueues.Add(returnRequest);
            await _context.SaveChangesAsync();
            await _loggingService.LogEvent(User.Identity.Name, "Akcja ProcessReturn zakonczona pomyślnie - prośba o zwrot książki została złożona");
            TempData["SuccessMessage"] = "Prośba o zwrot książki została złożona.";
            return RedirectToAction("ReturnBook");
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        public async Task<IActionResult> ManageReturns()
        {
            var returnRequests = await _context.ReturnQueues
                .Include(rq => rq.Book)
                .ToListAsync();
            await _loggingService.LogEvent(User.Identity.Name, "Akcja ManageReturns zakonczona pomyślnie");
            return View(returnRequests);
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpPost]
        public async Task<IActionResult> AcceptReturn(int requestId)
        {
            var returnRequest = await _context.ReturnQueues
                .Include(rq => rq.Book)
                .FirstOrDefaultAsync(rq => rq.Id == requestId);

            if (returnRequest != null)
            {
                var rental = await _context.Rentals
                    .FirstOrDefaultAsync(r => r.BookID == returnRequest.BookId && r.UserLogin == returnRequest.UserLogin);

                if (rental != null)
                {
                    _context.Rentals.Remove(rental);

                    var book = await _context.Books
                        .FirstOrDefaultAsync(b => b.ID == returnRequest.BookId);
                    if (book != null)
                    {
                        book.AvailableCopies++;
                    }
                }

                _context.ReturnQueues.Remove(returnRequest);
                await _context.SaveChangesAsync();
                await _loggingService.LogEvent(User.Identity.Name, "Akcja AcceptReturn zakonczona pomyślnie - zwrot książki został zaakceptowany");
                TempData["SuccessMessage"] = "Zwrot książki został zaakceptowany.";
            }
            else
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja AcceptReturn zakonczona niepowodzeniem - nie znaleziono żądania zwrotu");
                TempData["ErrorMessage"] = "Nie znaleziono żądania zwrotu.";
            }

            return RedirectToAction("ManageReturns");
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        public async Task<IActionResult> DeleteBook(string query)
        {
            var booksToDelete = _context.Books.ToList();

            if (!string.IsNullOrEmpty(query))
            {
                booksToDelete = booksToDelete.Where(b => b.Title.Contains(query) || b.Author.Contains(query)).ToList();
            }

            ViewData["Query"] = query;

            return View(booksToDelete);
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpPost]
        public async Task<IActionResult> DeleteBookConfirmed(int bookId, string query)
        {
            var bookToDelete = await _context.Books.FindAsync(bookId);
            if (bookToDelete == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono książki.";
                return NotFound();
            }

            _context.Books.Remove(bookToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Książka została pomyślnie usunięta.";
            await _loggingService.LogEvent(User.Identity.Name, "Akcja DeleteBookConfirmed zakonczona pomyślnie");
            return RedirectToAction(nameof(DeleteBook), new { query });
        }
        [Authorize(Roles = "admin,bibliotekarz")]
        [HttpPost]
        public async Task<IActionResult> RejectReturn(int requestId)
        {
            var returnRequest = await _context.ReturnQueues
                .Include(rq => rq.Book)
                .FirstOrDefaultAsync(rq => rq.Id == requestId);

            if (returnRequest != null)
            {
                _context.ReturnQueues.Remove(returnRequest);
                await _context.SaveChangesAsync();
                await _loggingService.LogEvent(User.Identity.Name, "Akcja RejectReturn została pomyślnie wykonana!");
                TempData["SuccessMessage"] = "Prośba o zwrot książki została odrzucona.";
            }
            else
            {
                await _loggingService.LogEvent(User.Identity.Name, "Akcja RejectReturn zakończona niepowodzeniem - nie znaleziono żądania zwrotu");
                TempData["ErrorMessage"] = "Nie znaleziono żądania zwrotu.";
            }

            return RedirectToAction("ManageReturns");
        }

    }
}

