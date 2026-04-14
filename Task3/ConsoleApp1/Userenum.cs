using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public enum Userenum
    {
        None,

        //Detail
        detail_show,
        detail_delete,

        detail_code,
        detail_decimal_number,
        detail_name,
        detail_alloy_grade,
        detail_mass,

        detail_code_update,
        detail_decimal_number_update,
        detail_name_update,
        detail_alloy_grade_update,
        detail_mass_update,

        //Operations
        operation_show,
        operation_delete,

        operation_code,
        operation_workshop_number,
        operation_duration_hours,
        operation_cost,

        operation_code_update,
        operation_workshop_number_update,
        operation_duration_hours_update,
        operation_cost_update,

        //production
        production_show,
        production_update,
        production_delete,

        production_detail_code,
        production_operation_number_in_process,
        production_operation_code,

        production_detail_code_delete,
        production_operation_number_in_process_delete,


    }
}
