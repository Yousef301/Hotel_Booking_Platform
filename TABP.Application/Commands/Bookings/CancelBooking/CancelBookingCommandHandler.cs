﻿using MediatR;
using TABP.DAL.Interfaces;
using TABP.DAL.Interfaces.Repositories;
using TABP.Domain.Enums;
using TABP.Domain.Exceptions;

namespace TABP.Application.Commands.Bookings.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork,
        IPaymentRepository paymentRepository)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _paymentRepository = paymentRepository;
    }

    public async Task Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, true)
                      ?? throw new NotFoundException(
                          $"Booking with id {request.BookingId} wasn't found");

        if (booking.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not allowed to cancel this booking");
        }

        if (booking.BookingStatus != BookingStatus.Confirmed)
        {
            throw new BookingStatusException("Booking cannot be cancelled");
        }

        booking.BookingStatus = BookingStatus.Cancelled;
        booking.PaymentStatus = PaymentStatus.Refunded;
        booking.Payment.PaymentStatus = PaymentStatus.Refunded;

        await _bookingRepository.UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync();
    }
}