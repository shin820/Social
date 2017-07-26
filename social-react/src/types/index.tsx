export class FilterListItem {
    name: string;
    unReadNum: number;
    id: number;
    constructor(id: number, name: string, unReadNum: number) {
        this.id = id;
        this.name = name;
        this.unReadNum = unReadNum;
    }
}

export interface ApplicationState {
    FilterList: FilterListItem[]
}